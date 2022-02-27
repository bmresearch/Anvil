using Anvil.Core.ViewModels;
using Anvil.Models;
using Anvil.Services;
using Anvil.Services.Network;
using Anvil.Services.Store.Models;
using Anvil.Services.Wallets.SubWallets;
using Anvil.ViewModels.Fields;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Solnet.Extensions;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Programs.Models.TokenProgram;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Crafter
{
    /// <summary>
    /// The view model used to craft transactions.
    /// </summary>
    public class TransactionCraftViewModel : ViewModelBase
    {
        private static readonly PublicKey WrappedSolMint = new ("So11111111111111111111111111111111111111112");

        #region Framework

        private IClassicDesktopStyleApplicationLifetime _appLifetime;

        #endregion

        private IRpcClientProvider _rpcProvider;
        private IRpcClient _rpcClient => _rpcProvider.Client;
        private IWalletService _walletService;
        private INonceAccountMappingStore _nonceAccountMappingStore;

        private InternetConnectionService _internetConnectionService;
        private AddressBookService _addressBookService;
        private TokenWallet _tokenWallet;
        private TokenMintResolver _tokenMintResolver;

        private PublicKey _sourceAta;
        private PublicKey _destinationAta;

        private TransactionBuilder _txBuilder;
        private byte[] _msgBytes;

        public TransactionCraftViewModel(IClassicDesktopStyleApplicationLifetime appLifetime,
            InternetConnectionService internetConnectionService,
            IRpcClientProvider rpcProvider, IWalletService walletService,
            INonceAccountMappingStore nonceAccountMappingStore, AddressBookService addressBookService)
        {
            _appLifetime = appLifetime;
            _internetConnectionService = internetConnectionService;
            _internetConnectionService.OnNetworkConnectionChanged += OnNetworkConnectionChanged;
            _rpcProvider = rpcProvider;
            _walletService = walletService;
            _nonceAccountMappingStore = nonceAccountMappingStore;
            _addressBookService = addressBookService;

            SourceAccount = new PublicKeyViewModel();
            DestinationAccount = new PublicKeyViewModel();
            NoConnection = !_internetConnectionService.IsConnected;

            this.WhenAnyValue(x => x.SourceAccount.PublicKey)
                .Subscribe(async x =>
                {
                    if (x != null)
                    {
                        await GetSourceAccount();
                    }
                    else
                    {
                        if (AccountContent != null)
                        {
                            AccountContent.PropertyChanged -= AccountContent_PropertyChanged;
                            AccountContent = null;
                        }
                        NonceAccountExists = false;
                        SourceInput = false;
                    }
                    this.RaisePropertyChanged(nameof(CanCraftTransaction));
                });

            this.WhenAnyValue(x => x.DestinationAccount.PublicKey)
                .Subscribe(async x =>
                {
                    if (x != null)
                    {
                        await GetDestinationAccount();
                    }
                    else
                    {
                        DestinationInput = false;
                    }
                    this.RaisePropertyChanged(nameof(CanCraftTransaction));
                });
        }

        private void OnNetworkConnectionChanged(object? sender, Services.Network.Events.NetworkConnectionChangedEventArgs e)
        {
            NoConnection = !e.Connected;
        }

        public async void SaveTransaction()
        {
            var ofd = new SaveFileDialog()
            {
                Title = "Save Transaction To File",
                DefaultExtension = "tx"
            };
            var selected = await ofd.ShowAsync(_appLifetime.MainWindow);
            if (selected == null) return;

            await File.WriteAllTextAsync(selected, Payload);
        }

        public void EditTransaction()
        {
            TransactionCrafted = false;
        }

        public void CopyTransactionToClipboard()
        {
            Application.Current.Clipboard.SetTextAsync(Payload);
        }

        public void CraftNewTransaction()
        {
            SourceAccount.Clear();
            DestinationAccount.Clear();

            if (AccountContent != null)
            {
                AccountContent.PropertyChanged -= AccountContent_PropertyChanged;
                AccountContent = null;
            }
            NonceAccountExists = false;
            SourceInput = false;
            DestinationInput = false;
            TransactionCrafted = false;
        }

        /// <summary>
        /// Crafts the transaction.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        public async Task CraftTransaction()
        {
            if (NonceAccountViewModel == null)
            {
                // something went wrong
                TransactionCrafted = false;
                TransactionCraftingError = true;
                return;
            }

            // initialize the nonce information to be used with the transaction
            var nonceInfo = new NonceInformation()
            {
                Nonce = NonceAccountViewModel.Nonce,
                Instruction = SystemProgram.AdvanceNonceAccount(
                    new(NonceAccountViewModel.NonceAccountMap.Account),
                    new(NonceAccountViewModel.NonceAccountMap.Authority)
                )
            };

            if (AccountContent == null)
            {
                // something went wrong
                TransactionCrafted = false;
                TransactionCraftingError = true;
                return;
            }

            _destinationAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                DestinationAccount.PublicKey,
                new(AccountContent.SelectedAsset.TokenMint));
            _sourceAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                SourceAccount.PublicKey,
                new(AccountContent.SelectedAsset.TokenMint));

            if (AccountContent is MultiSignatureAccountViewModel multiSigVm)
            {
                // because the source is a multisig we'll set the current wallet as the fee payer
                _txBuilder = new TransactionBuilder()
                    .SetFeePayer(_walletService.CurrentWallet.Address)
                    .SetNonceInformation(nonceInfo);
                await CraftMultiSignatureTransaction(multiSigVm.SelectedSigners);
            }
            else
            {
                // because the source is a regular account it can be the fee payer
                _txBuilder = new TransactionBuilder()
                    .SetFeePayer(SourceAccount.PublicKey)
                    .SetNonceInformation(nonceInfo);
                await CraftNonMultiSignatureTransaction();
            }

            _msgBytes = _txBuilder.CompileMessage();
            Payload = Convert.ToBase64String(_msgBytes);
            TransactionCrafted = true;
        }

        public async void CreateNonceAccount()
        {
            CreatingNonceAccount = true;
            var blockHash = await _rpcClient.GetRecentBlockHashAsync();
            var rentExemption = await _rpcClient.GetMinimumBalanceForRentExemptionAsync(NonceAccount.AccountDataSize);

            var newNonceAccount = new Account();
            PublicKey authority = AccountContent is MultiSignatureAccountViewModel multiSigVm ? _walletService.CurrentWallet.Address : SourceAccount.PublicKey;

            var txBuilder = new TransactionBuilder()
                .SetFeePayer(_walletService.CurrentWallet.Address)
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .AddInstruction(SystemProgram.CreateAccount(
                    _walletService.CurrentWallet.Address,
                    newNonceAccount,
                    rentExemption.Result,
                    NonceAccount.AccountDataSize,
                    SystemProgram.ProgramIdKey
                ))
                .AddInstruction(SystemProgram.InitializeNonceAccount(newNonceAccount, authority));
            var msgBytes = txBuilder.CompileMessage();

            txBuilder.AddSignature(_walletService.CurrentWallet.Sign(msgBytes));
            txBuilder.AddSignature(newNonceAccount.Sign(msgBytes));

            // submit create nonce and poll confirmation
            var txSign = await _rpcClient.SendTransactionAsync(txBuilder.Serialize());

            if (txSign.WasSuccessful)
            {
                ErrorCreatingAccount = false;
                ErrorCreatingAccountMessage = string.Empty;
                // and poll confirmation
                _ = await _rpcProvider.PollTxAsync(txSign.Result, Solnet.Rpc.Types.Commitment.Confirmed);

                // once confirmed add to local storage
                _nonceAccountMappingStore.AddMapping(new NonceAccountMapping
                {
                    Account = newNonceAccount.PublicKey,
                    Authority = authority
                });

                await Task.Delay(500);

                // fetch nonce account again
                var _mapping = _nonceAccountMappingStore.GetMapping(authority);
                var _nonceAccount = await GetNonceAccount(_mapping.Account);
                CreatingNonceAccount = false;
                NonceAccountViewModel = new(_nonceAccount, _mapping);
                NonceAccountExists = true;
            }
            else
            {
                CreatingNonceAccount = false;
                ErrorCreatingAccount = true;
                ErrorCreatingAccountMessage = txSign.Reason;
            }
        }

        /// <summary>
        /// Creates a token account for a given owner and token mint.
        /// </summary>
        /// <param name="owner">The token account owner.</param>
        /// <param name="owner">The token mint.</param>
        public async void CreateTokenAccount(PublicKey owner, PublicKey mint)
        {
            var blockHash = await _rpcClient.GetRecentBlockHashAsync();

            var txBuilder = new TransactionBuilder()
                .SetFeePayer(_walletService.CurrentWallet.Address)
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .AddInstruction(AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                    _walletService.CurrentWallet.Address,
                    owner,
                    mint));

            var msgBytes = txBuilder.CompileMessage();

            txBuilder.AddSignature(_walletService.CurrentWallet.Sign(msgBytes));

            // submit transaction to create nonce
            var txSign = await _rpcClient.SendTransactionAsync(txBuilder.Serialize());

            CreatingTokenAccount = true;

            await Task.Delay(2500);

            // and poll confirmation
            var txMeta = await _rpcProvider.PollTxAsync(txSign.Result, Solnet.Rpc.Types.Commitment.Confirmed);

            var ata = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(owner, mint);

            // fetch token account again
            var tokenAccount = await GetTokenAccount(ata);

            CreatingTokenAccount = false;
        }

        /// <summary>
        /// Adds instructions to send solana from a regular account to a multisig account.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        private async Task SendSolanaFromRegularToMultiSig()
        {
            // sanity check, this should never happen
            if (AccountContent == null)
            {
                TransactionCrafted = false;
                TransactionCraftingError = true;
                return;
            }

            var destinationAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(DestinationAccount.PublicKey, new(AccountContent.SelectedAsset.TokenMint));
            // destination account is a multisig so we have to transfer wrapped sol to the multisig's wrapped sol associated token account
            var destinationTokenAccount = await GetTokenAccount(destinationAta);

            if (destinationTokenAccount == null)
            {
                // destination associated token account doesn't exist so we have to create it and set the source account as the rent payer
                _txBuilder.AddInstruction(AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                    SourceAccount.PublicKey,
                    DestinationAccount.PublicKey,
                    new(AccountContent.SelectedAsset.TokenMint)));
            }

            // now we'll wrap sol, transfer it and then close our wrapped sol token account

            _txBuilder.AddInstruction(SystemProgram.Transfer(
                SourceAccount.PublicKey,
                destinationAta,
                SolHelper.ConvertToLamports(AccountContent.Amount)))
                .AddInstruction(TokenProgram.SyncNative(destinationAta));
        }

        /// <summary>
        /// Adds instructions to send tokens from a regular account.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        private async Task SendTokenFromRegular()
        {
            // sanity check, this should never happen
            if (AccountContent == null)
            {
                TransactionCrafted = false;
                TransactionCraftingError = true;
                return;
            }
            var destinationAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(DestinationAccount.PublicKey, new(AccountContent.SelectedAsset.TokenMint));
            var destinationTokenAccount = await GetTokenAccount(destinationAta);

            if (destinationTokenAccount == null)
            {
                // destination associated token account doesn't exist so we have to create it and set the source account as the rent payer
                _txBuilder.AddInstruction(AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                    SourceAccount.PublicKey,
                    DestinationAccount.PublicKey,
                    new(AccountContent.SelectedAsset.TokenMint)));
            }

            var amountConverter = (decimal)Math.Pow(10, AccountContent.SelectedAsset.Decimals);
            var amount = (ulong)(AccountContent.Amount * amountConverter);
            _txBuilder.AddInstruction(TokenProgram.Transfer(
                _sourceAta,
                _destinationAta,
                amount,
                SourceAccount.PublicKey));
        }

        /// <summary>
        /// Adds instructions to a non-multisig transaction.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        private async Task CraftNonMultiSignatureTransaction()
        {
            // sanity check, this should never happen
            if (AccountContent == null)
            {
                TransactionCrafted = false;
                TransactionCraftingError = true;
                return;
            }

            if (AccountContent.SelectedAsset.TokenName == "Solana")
            {
                if (DestinationMultiSig)
                {
                    await SendSolanaFromRegularToMultiSig();
                }
                else
                {
                    // destination account is not a multisig so we'll just do a regular lamports transfer
                    _txBuilder.AddInstruction(SystemProgram.Transfer(
                        SourceAccount.PublicKey,
                        DestinationAccount.PublicKey,
                        SolHelper.ConvertToLamports(AccountContent.Amount)
                    ));
                }
            }
            else
            {
                await SendTokenFromRegular();
            }
        }

        /// <summary>
        /// Adds instructions to a multisig transaction.
        /// </summary>
        private async Task CraftMultiSignatureTransaction(ObservableCollection<PublicKey> selectedSigners)
        {
            // sanity check, this should never happen
            if (AccountContent == null)
            {
                TransactionCrafted = false;
                TransactionCraftingError = true;
                return;
            }

            /// check if destination ATA exists, if it doesn't add instruction to create ATA
            /// this instruction is funded by the wallet service's current wallet
            var destinationTokenAccount = await _rpcClient.GetTokenAccountInfoAsync(_destinationAta,
                Solnet.Rpc.Types.Commitment.Confirmed);

            if (destinationTokenAccount.WasSuccessful)
            {
                if (destinationTokenAccount.Result.Value == null)
                    _txBuilder.AddInstruction(AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                        _walletService.CurrentWallet.Address,
                        DestinationAccount.PublicKey,
                        new(AccountContent.SelectedAsset.TokenMint)));
            }
            // if source Ata exists perform sync native and token program transfer to destination ATA

            var amountConverter = (decimal)Math.Pow(10, AccountContent.SelectedAsset.Decimals);
            var amount = (ulong)(AccountContent.Amount * amountConverter);
            if (AccountContent.SelectedAsset.TokenName == "Solana")
            {
                _txBuilder
                    .AddInstruction(TokenProgram.SyncNative(_sourceAta))
                    .AddInstruction(TokenProgram.Transfer(
                        _sourceAta,
                        _destinationAta,
                        amount,
                        SourceAccount.PublicKey,
                        selectedSigners));
            }
            else
            {
                _txBuilder
                    .AddInstruction(TokenProgram.Transfer(
                    _sourceAta,
                    _destinationAta,
                    amount,
                    SourceAccount.PublicKey,
                    selectedSigners));
            }
        }

        /// <summary>
        /// Gets the token account for a given owner and token mint.
        /// </summary>
        /// <param name="ata">The associated token account.</param>
        /// <returns>A task which performs the action and may return the token account.</returns>
        private async Task<TokenAccountInfo?> GetTokenAccount(PublicKey ata)
        {
            var tokenAccount = await _rpcClient.GetTokenAccountInfoAsync(ata, Solnet.Rpc.Types.Commitment.Confirmed);
            if (tokenAccount.WasSuccessful)
            {
                if (tokenAccount.Result.Value == null) return null;
                return tokenAccount.Result.Value;
            }
            return null;
        }

        /// <summary>
        /// Gets a nonce account with a given public key.
        /// </summary>
        /// <param name="accountKey">The account public key.</param>
        /// <returns>A task which performs the action and may return the nonce account.</returns>
        private async Task<NonceAccount?> GetNonceAccount(string accountKey)
        {
            var nonceAccountInfo = await _rpcClient.GetAccountInfoAsync(accountKey, Solnet.Rpc.Types.Commitment.Confirmed);
            if (nonceAccountInfo.WasSuccessful)
            {
                if (nonceAccountInfo.Result.Value == null) return null;
                byte[] accountDataBytes = Convert.FromBase64String(nonceAccountInfo.Result.Value.Data[0]);

                if (accountDataBytes.Length != NonceAccount.AccountDataSize) return null;
                var nonceAccount = NonceAccount.Deserialize(accountDataBytes);

                return nonceAccount;
            }
            return null;
        }

        /// <summary>
        /// Get the source account to check whether it's a regular account or a multisig.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        private async Task GetSourceAccount()
        {
            await GetAccountInfo(SourceAccount.PublicKey).ContinueWith(async account =>
            {
                if (account.Result != null)
                {
                    MultiSignatureAccount? _multiSigAccount = null;
                    NonceAccountMapping? _mapping = null;

                    // attempt to deserialize the account data into the multisig account structure
                    var _sourceAccountData = account.Result.Data[0];
                    var accountBytes = Convert.FromBase64String(_sourceAccountData);
                    if (accountBytes.Length == MultiSignatureAccount.Layout.Length)
                    {
                        _multiSigAccount = MultiSignatureAccount.Deserialize(accountBytes);
                    }

                    var assets = await GetTokenAccounts(SourceAccount.PublicKey);
                    if (assets == null) return;

                    if (_multiSigAccount != null)
                    {
                        // because the source is multi sig the current wallet needs to sign to advance the nonce
                        _mapping = _nonceAccountMappingStore.GetMapping(_walletService.CurrentWallet.Address);
                        AccountContent = new MultiSignatureAccountViewModel(assets, _multiSigAccount);
                        AccountContent.PropertyChanged += AccountContent_PropertyChanged;
                    }
                    else
                    {
                        // the source account is a regular account so it can be the authority of the nonce account
                        _mapping = _nonceAccountMappingStore.GetMapping(SourceAccount.PublicKey);
                        AccountContent = new AccountViewModel(assets);
                        AccountContent.PropertyChanged += AccountContent_PropertyChanged;
                    }

                    if (_mapping != null)
                    {
                        var nonceAccount = await GetNonceAccount(_mapping.Account);
                        if (nonceAccount != null)
                        {
                            NonceAccountViewModel = new(nonceAccount, _mapping);
                            NonceAccountExists = true;
                        }
                        else
                        {
                            // for some reason we couldn't fetch the nonce account so we need to trigger an error here
                            NonceAccountViewModel = null;
                            NonceAccountExists = false;
                            SourceInput = false;
                            return;
                        }
                    }
                    else
                    {
                        NonceAccountViewModel = null;
                        NonceAccountExists = false;
                    }
                    SourceInput = true;
                }
            });
        }

        /// <summary>
        /// Get the destination account to check whether it's a regular account or a multisig.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        private async Task GetDestinationAccount()
        {
            await GetAccountInfo(DestinationAccount.PublicKey).ContinueWith(async account =>
            {
                if (account.Result != null)
                {
                    MultiSignatureAccount? _multiSigAccount = null;

                    // attempt to deserialize the account data into the multisig account structure
                    var _destAccountData = account.Result.Data[0];
                    var accountBytes = Convert.FromBase64String(_destAccountData);
                    if (accountBytes.Length == MultiSignatureAccount.Layout.Length)
                    {
                        _multiSigAccount = MultiSignatureAccount.Deserialize(accountBytes);
                    }

                    var assets = await GetTokenAccounts(DestinationAccount.PublicKey);
                    if (assets == null) return;

                    if (_multiSigAccount != null)
                    {
                        // because the destination is multi sig we need to flag it as such so as not to transfer lamports to the account
                        // but rather to an associated token account
                        DestinationMultiSig = true;
                    }
                    else
                    {
                        // the destination account is a regular account so it can be the authority of the nonce account
                        DestinationMultiSig = false;
                    }

                    DestinationInput = true;
                }
            });
        }

        /// <summary>
        /// Handles changes to the account content properties.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void AccountContent_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null) return;
            if (e.PropertyName.Contains("Validated"))
                this.RaisePropertyChanged(nameof(CanCraftTransaction));
        }

        /// <summary>
        /// Gets the token accounts for the account with the given public key.
        /// </summary>
        /// <param name="accountKey">The token accounts owner's public key.</param>
        /// <param name="isMultiSig">Whether the token accounts owner is a multisig or not.</param>
        /// <returns>A task which performs the action and may return a collection of token wallet balances.</returns>
        private async Task<ObservableCollection<TokenWalletBalanceWrapper>?> GetTokenAccounts(PublicKey accountKey, bool isMultiSig = false)
        {
            _tokenMintResolver ??= await TokenMintResolver.LoadAsync();
            _tokenWallet = await TokenWallet.LoadAsync(_rpcClient, _tokenMintResolver, accountKey);

            TokenWalletBalanceWrapper? solanaTokenWrapper = null;

            if (isMultiSig)
            {
                // in case the account is a multisig we need to get the wrapped sol ata and not use the actual multisig's balance in lamports
                var ata = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(accountKey, WrappedSolMint);
                var tokenAccount = await GetTokenAccount(ata);
                if (tokenAccount == null) return null;




            } else
            {
                // this account is not a multisig so we'll just use the account's balance in lamports
                var balance = await GetBalance(accountKey);
                if (balance == null) return null;
                
                solanaTokenWrapper = new TokenWalletBalanceWrapper("Solana",
                    balance.Value,
                    SolHelper.ConvertToSol(balance.Value),
                    10,
                    WrappedSolMint);
            }

            if (solanaTokenWrapper == null) return null;

            var assets = new ObservableCollection<TokenWalletBalanceWrapper>() 
            {
                solanaTokenWrapper
            };

            foreach (var twb in _tokenWallet.Balances())
            {
                assets.Add(new TokenWalletBalanceWrapper(twb));
            }
            return assets;
        }

        /// <summary>
        /// Gets the native balance for the account with the given public key.
        /// </summary>
        /// <param name="accountKey">The account public key.</param>
        /// <returns>A task which performs the action and may return the account balance.</returns>
        private async Task<ulong?> GetBalance(string accountKey)
        {
            var account = await _rpcClient.GetBalanceAsync(accountKey, Solnet.Rpc.Types.Commitment.Confirmed);
            if (account.WasSuccessful)
            {
                return account.Result.Value;
            }
            return null;
        }

        /// <summary>
        /// Gets the account info for the account with the given public key.
        /// </summary>
        /// <param name="accountKey">The account public key.</param>
        /// <returns>A task which performs the action and may return the account info.</returns>
        private async Task<AccountInfo?> GetAccountInfo(string accountKey)
        {
            var account = await _rpcClient.GetAccountInfoAsync(accountKey, Solnet.Rpc.Types.Commitment.Confirmed);
            if (account.WasSuccessful)
            {
                if (account.Result.Value == null) return null;

                return account.Result.Value;
            }

            return null;
        }

        private AddressBookItem _selectedAddressBookItem;
        public AddressBookItem SelectedAddressBookItem
        {
            get => _selectedAddressBookItem;
            set => this.RaiseAndSetIfChanged(ref _selectedAddressBookItem, value);
        }

        public List<AddressBookItem> AddressBookItems
        {
            get => _addressBookService.GetItems();
        }

        private PublicKeyViewModel _sourceAccount;
        public PublicKeyViewModel SourceAccount
        {
            get => _sourceAccount;
            set => this.RaiseAndSetIfChanged(ref _sourceAccount, value);
        }

        private PublicKeyViewModel _destinationAccount;
        public PublicKeyViewModel DestinationAccount
        {
            get => _destinationAccount;
            set => this.RaiseAndSetIfChanged(ref _destinationAccount, value);
        }

        private string _payload;
        public string Payload
        {
            get => _payload;
            set => this.RaiseAndSetIfChanged(ref _payload, value);
        }

        private bool _transactionCrafted;
        public bool TransactionCrafted
        {
            get => _transactionCrafted;
            set => this.RaiseAndSetIfChanged(ref _transactionCrafted, value);
        }

        private bool _noConnection;
        public bool NoConnection
        {
            get => _noConnection;
            set => this.RaiseAndSetIfChanged(ref _noConnection, value);
        }

        private bool _transactionCraftingError;
        public bool TransactionCraftingError
        {
            get => _transactionCraftingError;
            set => this.RaiseAndSetIfChanged(ref _transactionCraftingError, value);
        }

        private bool _sourceInput;
        public bool SourceInput
        {
            get => _sourceInput;
            set => this.RaiseAndSetIfChanged(ref _sourceInput, value);
        }

        private bool _destinationInput;
        public bool DestinationInput
        {
            get => _destinationInput;
            set => this.RaiseAndSetIfChanged(ref _destinationInput, value);
        }

        private bool _destinationMultiSig;
        public bool DestinationMultiSig
        {
            get => _destinationMultiSig;
            set => this.RaiseAndSetIfChanged(ref _destinationMultiSig, value);
        }

        private bool _nonceAccountExists;
        public bool NonceAccountExists
        {
            get => _nonceAccountExists;
            set => this.RaiseAndSetIfChanged(ref _nonceAccountExists, value);
        }

        private bool _creatingNonceAccount;
        public bool CreatingNonceAccount
        {
            get => _creatingNonceAccount;
            set => this.RaiseAndSetIfChanged(ref _creatingNonceAccount, value);
        }

        private bool _creatingTokenAccount;
        public bool CreatingTokenAccount
        {
            get => _creatingTokenAccount;
            set => this.RaiseAndSetIfChanged(ref _creatingTokenAccount, value);
        }

        private bool _errorCreatingAccount;
        public bool ErrorCreatingAccount
        {
            get => _errorCreatingAccount;
            set => this.RaiseAndSetIfChanged(ref _errorCreatingAccount, value);
        }

        private string _errorCreatingAccountMessage;
        public string ErrorCreatingAccountMessage
        {
            get => _errorCreatingAccountMessage;
            set => this.RaiseAndSetIfChanged(ref _errorCreatingAccountMessage, value);
        }

        private AccountViewModel? _accountContent;
        public AccountViewModel? AccountContent
        {
            get => _accountContent;
            set => this.RaiseAndSetIfChanged(ref _accountContent, value);
        }

        private NonceAccountViewModel? _nonceAccountViewModel;
        public NonceAccountViewModel? NonceAccountViewModel
        {
            get => _nonceAccountViewModel;
            set => this.RaiseAndSetIfChanged(ref _nonceAccountViewModel, value);
        }

        public bool CanCraftTransaction
        {
            get
            {
                if (AccountContent is MultiSignatureAccountViewModel multiSigVm)
                    return multiSigVm.Validated && SourceAccount.PublicKey != null && DestinationAccount.PublicKey != null;
                if (AccountContent is AccountViewModel accountVm)
                    return accountVm.InputValidated && SourceAccount.PublicKey != null && DestinationAccount.PublicKey != null;

                return false;
            }
        }

        public string Header => "Craft Transaction";
    }
}
