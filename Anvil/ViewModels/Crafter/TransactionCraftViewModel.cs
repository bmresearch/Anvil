using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.Services.Store.Models;
using Anvil.ViewModels.Fields;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Solnet.Extensions;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Rpc.Utilities;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Crafter
{
    public class TokenWalletBalanceWrapper
    {
        private TokenWalletBalance _tokenWalletBalance;
        private PublicKey _mint;
        private string _name;
        private ulong _rawBalance;
        private decimal _balance;
        private int _decimals;

        public TokenWalletBalanceWrapper(TokenWalletBalance walletBalance)
        {
            _tokenWalletBalance = walletBalance;
        }

        public TokenWalletBalanceWrapper(string name, ulong rawBalance, decimal balance, int decimals, PublicKey mint)
        {
            _name = name;
            _rawBalance = rawBalance;
            _balance = balance;
            _decimals = decimals;
            _mint = mint;
        }

        public int Decimals
        {
            get
            {
                return _tokenWalletBalance != null ? _tokenWalletBalance.DecimalPlaces : _decimals;
            }
        }

        public ulong RawBalance
        {
            get
            {
                return _tokenWalletBalance != null ? _tokenWalletBalance.QuantityRaw : _rawBalance;
            }
        }

        public decimal Balance
        {
            get
            {
                return _tokenWalletBalance != null ? _tokenWalletBalance.QuantityDecimal : _balance;
            }
        }

        public string TokenName
        {
            get
            {
                return _tokenWalletBalance != null ? _tokenWalletBalance.TokenName : _name;
            }
        }

        public string TokenMint
        {
            get
            {
                return _tokenWalletBalance != null ? _tokenWalletBalance.TokenMint : _mint;
            }
        }

        public TokenWalletBalance TokenWalletBalance { get => _tokenWalletBalance; }
    }

    public class TransactionCraftViewModel : ViewModelBase
    {
        private IRpcClientProvider _rpcProvider;
        private IRpcClient _rpcClient => _rpcProvider.Client;
        private IWalletService _walletService;
        private INonceAccountMappingStore _nonceAccountMappingStore;
        private TokenWallet _tokenWallet;
        private TokenMintResolver _tokenMintResolver;

        private PublicKey _destinationTokenAccount;

        private TransactionBuilder _txBuilder;
        private Transaction _tx;
        private Message _msg;
        private byte[] _msgBytes;

        public TransactionCraftViewModel(IRpcClientProvider rpcProvider, IWalletService walletService, INonceAccountMappingStore nonceAccountMappingStore)
        {
            _rpcProvider = rpcProvider;
            _walletService = walletService;
            _nonceAccountMappingStore = nonceAccountMappingStore;
            SourceAccount = new PublicKeyViewModel();
            DestinationAccount = new PublicKeyViewModel();

            this.WhenAnyValue(x => x.SourceAccount.PublicKey)
                .Subscribe(x =>
                {
                    if (x != null)
                    {
                        GetSourceAccount();
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
                    this.RaisePropertyChanged("CanCraftTransaction");
                });

            this.WhenAnyValue(x => x.DestinationAccount.PublicKey)
                .Subscribe(x =>
                {
                    if (x != null)
                    {
                    }
                    else
                    {
                    }
                    this.RaisePropertyChanged("CanCraftTransaction");
                });
        }        
        
        public void CopyTransactionToClipboard()
        {
            Application.Current.Clipboard.SetTextAsync(Payload);
        }

        public async void SaveTransaction()
        {
            var ofd = new SaveFileDialog()
            {
                Title = "Save Transaction To File",
                DefaultExtension = "tx"
            };
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var selected = await ofd.ShowAsync(desktop.MainWindow);
                if (selected == null) return;

                await File.WriteAllTextAsync(selected, Payload);
            }
        }

        public void EditTransaction()
        {
            TransactionCrafted = false;
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
            TransactionCrafted = false;
        }

        public void CraftTransaction()
        {
            // Initialize the nonce information to be used with the transaction
            NonceInformation nonceInfo = new NonceInformation()
            {
                Nonce = NonceAccountViewModel.Nonce,
                Instruction = SystemProgram.AdvanceNonceAccount(
                    new(NonceAccountViewModel.NonceAccountMap.Account),
                    new(NonceAccountViewModel.NonceAccountMap.Authority)
                )
            };
            _txBuilder = new TransactionBuilder()
                .SetFeePayer(SourceAccount.PublicKey)
                .SetNonceInformation(nonceInfo);

            if (AccountContent is MultiSignatureAccountViewModel multiSigVm)
            {
                CraftMultiSignatureTransaction(multiSigVm.SelectedSigners);
            }
            else
            {
                CraftNonMultiSignatureTransaction();
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
            PublicKey authority = AccountContent is MultiSignatureAccountViewModel multiSigVm ? _walletService.CurrentWallet.Wallet.Account.PublicKey : SourceAccount.PublicKey;

            var txBytes = new TransactionBuilder()
                .SetFeePayer(_walletService.CurrentWallet.Wallet.Account)
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .AddInstruction(SystemProgram.CreateAccount(
                    _walletService.CurrentWallet.Wallet.Account,
                    newNonceAccount,
                    rentExemption.Result,
                    NonceAccount.AccountDataSize,
                    SystemProgram.ProgramIdKey
                ))
                .AddInstruction(SystemProgram.InitializeNonceAccount(newNonceAccount, authority))
                .Build(new List<Account> { _walletService.CurrentWallet.Wallet.Account, newNonceAccount });

            // submit create nonce and poll confirmation
            var txSign = await _rpcClient.SendTransactionAsync(txBytes);

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

                // fetch nonce account again
                var _mapping = _nonceAccountMappingStore.GetMapping(authority);
                var _nonceAccount = await GetNonceAccount(_mapping.Account);
                CreatingNonceAccount = false;
                NonceAccountViewModel = new(_nonceAccount, _mapping);
            }
            else
            {
                CreatingNonceAccount = false;
                ErrorCreatingAccount = true;
                ErrorCreatingAccountMessage = txSign.Reason;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        public async void CreateTokenAccount(PublicKey owner, PublicKey mint)
        {
            var blockHash = await _rpcClient.GetRecentBlockHashAsync();

            var txBytes = new TransactionBuilder()
                .SetFeePayer(_walletService.CurrentWallet.Wallet.Account)
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .AddInstruction(AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(_walletService.CurrentWallet.Wallet.Account, owner, mint))
                .Build(new List<Account> { _walletService.CurrentWallet.Wallet.Account });

            // submit transaction to create nonce
            var txSign = await _rpcClient.SendTransactionAsync(txBytes);

            CreatingTokenAccount = true;

            Task.Delay(3000);
            // and poll confirmation
            var txMeta = await _rpcProvider.PollTxAsync(txSign.Result, Solnet.Rpc.Types.Commitment.Confirmed);

            // fetch token account again
            var tokenAccount = await GetTokenAccount(owner, mint);

            CreatingTokenAccount = false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CraftNonMultiSignatureTransaction()
        {
            if (AccountContent.SelectedAsset.TokenName == "Solana")
            {
                _txBuilder.AddInstruction(SystemProgram.Transfer(
                    SourceAccount.PublicKey,
                    DestinationAccount.PublicKey,
                    (ulong)(AccountContent.AssetAmount * SolHelper.LAMPORTS_PER_SOL)
                ));
            }
            else
            {
                var sourceAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(SourceAccount.PublicKey, new(AccountContent.SelectedAsset.TokenWalletBalance.TokenMint));
                _txBuilder.AddInstruction(TokenProgram.Transfer(
                    sourceAccount,
                    _destinationTokenAccount,
                    (ulong)(AccountContent.AssetAmount * Math.Pow(10, AccountContent.SelectedAsset.TokenWalletBalance.DecimalPlaces)),
                    SourceAccount.PublicKey));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CraftMultiSignatureTransaction(ObservableCollection<PublicKey> selectedSigners)
        {                
            // if source Ata exists perform sync native and token program transfer to destination ATA
            var sourceAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(SourceAccount.PublicKey, new(AccountContent.SelectedAsset.TokenWalletBalance.TokenMint));
            var destinationAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(DestinationAccount.PublicKey, new(AccountContent.SelectedAsset.TokenWalletBalance.TokenMint));

            if (AccountContent.SelectedAsset.TokenName == "Solana")
            {
                _txBuilder.AddInstruction(AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                        SourceAccount.PublicKey,
                        SourceAccount.PublicKey,
                        new(AccountContent.SelectedAsset.TokenWalletBalance.TokenMint)))
                    .AddInstruction(TokenProgram.Transfer(
                        sourceAta,
                        destinationAta,
                        (ulong)((double)AccountContent.AssetAmount / (ulong)AccountContent.SelectedAsset.Decimals),
                        DestinationAccount.PublicKey,
                        selectedSigners));
            }
            else
            {
                var sourceAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(SourceAccount.PublicKey, new(AccountContent.SelectedAsset.TokenWalletBalance.TokenMint));

                _txBuilder.AddInstruction(TokenProgram.Transfer(
                    sourceAccount,
                    _destinationTokenAccount,
                    (ulong)(AccountContent.AssetAmount * AccountContent.SelectedAsset.TokenWalletBalance.DecimalPlaces),
                    SourceAccount.PublicKey,
                    selectedSigners));
            }
        }


        private async Task<PublicKey> GetTokenAccount(PublicKey owner, PublicKey mint)
        {
            var tokenAccounts = await _rpcClient.GetTokenAccountsByOwnerAsync(owner, mint);
            if (tokenAccounts.WasSuccessful)
            {
                return new(tokenAccounts.Result.Value[0]?.PublicKey);
            }
            return null;
        }

        private async Task<NonceAccount> GetNonceAccount(string accountKey)
        {
            // Get the Nonce Account to get the Nonce to use for the transaction
            var nonceAccountInfo = await _rpcClient.GetAccountInfoAsync(accountKey, Solnet.Rpc.Types.Commitment.Confirmed);
            if (nonceAccountInfo.WasSuccessful)
            {
                byte[] accountDataBytes = Convert.FromBase64String(nonceAccountInfo.Result.Value.Data[0]);
                var nonceAccount = NonceAccount.Deserialize(accountDataBytes);
                return nonceAccount;
            }
            return null;
        }

        private async Task GetSourceAccount()
        {
            _tokenMintResolver ??= await TokenMintResolver.LoadAsync();
            await GetAccountInfo(SourceAccount.PublicKeyString).ContinueWith(async account =>
            {
                if (account.Result != null)
                {
                    MultiSignatureAccount _multiSigAccount = null;
                    NonceAccountMapping _mapping = null;
                    var _sourceAccountData = account.Result.Data[0];
                    var accountBytes = Convert.FromBase64String(_sourceAccountData);
                    if (accountBytes.Length == MultiSignatureAccount.Layout.Length)
                    {
                        _multiSigAccount = MultiSignatureAccount.Deserialize(accountBytes);
                    }

                    var assets = await GetTokenAccounts(SourceAccount.PublicKeyString);

                    if (_multiSigAccount != null)
                    {
                        // because the source is multi sig the current wallet needs to sign to advance the nonce
                        _mapping = _nonceAccountMappingStore.GetMapping(_walletService.CurrentWallet.Wallet.Account.PublicKey);
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
                        NonceAccountViewModel = new(nonceAccount, _mapping);
                        NonceAccountExists = true;
                    }
                    else
                    {
                        NonceAccountViewModel = null;
                    }
                    SourceInput = true;
                }
            });
        }

        private async Task GetDestinationAccount()
        {

        }

        private void AccountContent_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null) return;
            if (e.PropertyName.Contains("Validated"))
                this.RaisePropertyChanged("CanCraftTransaction");
        }

        private async Task<ObservableCollection<TokenWalletBalanceWrapper>> GetTokenAccounts(string accountKey)
        {
            _tokenWallet = await TokenWallet.LoadAsync(_rpcClient, _tokenMintResolver, new(accountKey));

            var balance = await GetBalance(accountKey);

            var solanaTokenWrapper =
                new TokenWalletBalanceWrapper("Solana", balance, (decimal)balance / SolHelper.LAMPORTS_PER_SOL, 10, new PublicKey("So11111111111111111111111111111111111111112"));
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

        private async Task<ulong> GetBalance(string accountKey)
        {
            var account = await _rpcClient.GetBalanceAsync(accountKey, Solnet.Rpc.Types.Commitment.Confirmed);

            return account.WasSuccessful ? account.Result.Value : 0;
        }

        private async Task<AccountInfo> GetAccountInfo(string accountKey)
        {
            var account = await _rpcClient.GetAccountInfoAsync(accountKey, Solnet.Rpc.Types.Commitment.Confirmed);

            return account.WasSuccessful ? account.Result.Value : null;
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

        private bool _sourceInput;
        public bool SourceInput
        {
            get => _sourceInput;
            set => this.RaiseAndSetIfChanged(ref _sourceInput, value);
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

        private AccountViewModel _accountContent;
        public AccountViewModel AccountContent
        {
            get => _accountContent;
            set => this.RaiseAndSetIfChanged(ref _accountContent, value);
        }

        private NonceAccountViewModel _nonceAccountViewModel;
        public NonceAccountViewModel NonceAccountViewModel
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
