using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.Services.Network;
using Anvil.Services.Wallets;
using Anvil.Services.Wallets.Enums;
using Anvil.Services.Wallets.SubWallets;
using Anvil.ViewModels.Common;
using Anvil.ViewModels.Dialogs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Material.Dialog;
using ReactiveUI;
using Solnet.Extensions;
using Solnet.Programs;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using Solnet.Wallet.Bip39;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Wallet
{
    /// <summary>
    /// The wallet view model.
    /// </summary>
    public class WalletViewModel : ViewModelBase
    {
        #region Framework

        private IClassicDesktopStyleApplicationLifetime _appLifetime;

        #endregion

        #region Application Services And Modules


        private IWalletService _walletService;
        private IRpcClientProvider _rpcClientProvider;
        private IRpcClient _rpcClient => _rpcClientProvider.Client;
        private ITokenMintResolver _resolver;

        private KeyStoreService _keyStoreService;
        private AddressBookService _addressBookService;
        private InternetConnectionService _internetConnectionService;

        #endregion

        public WalletViewModel(IClassicDesktopStyleApplicationLifetime appLifetime,
            InternetConnectionService internetConnectionService,
            IWalletService walletService, IRpcClientProvider rpcClientProvider,
            KeyStoreService keyStoreService, AddressBookService addressBookService)
        {
            _appLifetime = appLifetime;
            _internetConnectionService = internetConnectionService;
            _internetConnectionService.OnNetworkConnectionChanged += OnNetworkConnectionChanged;
            _rpcClientProvider = rpcClientProvider;
            _rpcClientProvider.OnClientChanged += OnClientChanged;
            _walletService = walletService;
            _walletService.OnCurrentWalletChanged += OnCurrentWalletChanged;
            _walletService.OnWalletServiceStateChanged += OnWalletServiceStateChanged;
            _keyStoreService = keyStoreService;
            _addressBookService = addressBookService;

            DerivationWalletsCollection = new();
            PrivateKeyWalletsCollection = new();
            NoConnection = !_internetConnectionService.IsConnected;
            TransactionSubmission = TransactionSubmissionViewModel.NoShow();
            TransactionSubmission.WhenAnyValue(x => x.SubmittingTransaction)
                .Subscribe(x =>
                {
                    CanSubmitTransaction = !x;
                });

            RestoreFromWalletSnapshot(false);
        }

        private void OnNetworkConnectionChanged(object sender, Services.Network.Events.NetworkConnectionChangedEventArgs e)
        {
            NoConnection = !e.Connected;
        }

        private async Task<SendSolanaDialogViewModel> GetDestinationAddressAndAmount()
        {
            var vm = new SendSolanaDialogViewModel()
            {
                NativeBalance = CurrentNativeBalance,
                AddressBookService = _addressBookService,
            };
            var dialog = DialogHelper.CreateCustomDialog(new CustomDialogBuilderParams
            {
                Borderless = true,
                Content = vm,
                StartupLocation = WindowStartupLocation.CenterOwner,
                Width = 500,
            });
            await dialog.ShowDialog(_appLifetime.MainWindow);
            if (!vm.Confirmed) return null;
            return vm;
        }

        private async Task<SendTokenDialogViewModel> GetDestinationAddressAndTokenInfo()
        {
            var vm = new SendTokenDialogViewModel()
            {
                Tokens = TokenBalances.ToList(),
                SelectedToken = TokenBalances.First(),
                AddressBookService = _addressBookService,
            };
            var dialog = DialogHelper.CreateCustomDialog(new CustomDialogBuilderParams
            {
                Borderless = true,
                Content = vm,
                StartupLocation = WindowStartupLocation.CenterOwner,
                Width = 500,
            });
            await dialog.ShowDialog(_appLifetime.MainWindow);
            if (!vm.Confirmed) return null;
            return vm;
        }


        public async void SendSolana()
        {
            var vm = await GetDestinationAddressAndAmount();
            if (vm == null) return;

            TransactionSubmission = new(_rpcClientProvider);
            TransactionSubmission.WhenAnyValue(x => x.SubmittingTransaction)
                .Subscribe(x =>
                {
                    CanSubmitTransaction = !x;
                });

            var blockHash = await _rpcClient.GetRecentBlockHashAsync();

            var txBuilder = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(CurrentWallet.Address)
                .AddInstruction(SystemProgram.Transfer(
                    CurrentWallet.Address,
                    vm.Destination.PublicKey,
                    SolHelper.ConvertToLamports(vm.Amount)));

            var txBytes = txBuilder.CompileMessage();

            var signature = CurrentWallet.Sign(txBytes);

            txBuilder.AddSignature(signature);

            var success = await TransactionSubmission.SubmitTransaction(txBuilder.Serialize());

            if (success)
            {
                await TransactionSubmission.PollConfirmation();
                await GetAccountHoldings();
                await Task.Delay(15000);
                TransactionSubmission = TransactionSubmissionViewModel.NoShow();
            }
            else
            {
                await Task.Delay(15000);
                TransactionSubmission = TransactionSubmissionViewModel.NoShow();
            }
        }

        private async void SendTokens()
        {
            var vm = await GetDestinationAddressAndTokenInfo();
            if (vm == null) return;

            TransactionSubmission = new(_rpcClientProvider);
            TransactionSubmission.WhenAnyValue(x => x.SubmittingTransaction)
                .Subscribe(x =>
                {
                    CanSubmitTransaction = !x;
                });

            var blockHash = await _rpcClient.GetRecentBlockHashAsync();

            var destinationAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                vm.Destination.PublicKey,
                new(vm.SelectedToken.TokenMint));
            var sourceAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                CurrentWallet.Address,
                new(vm.SelectedToken.TokenMint));

            var destinationTokenAccount = await _rpcClient.GetTokenAccountInfoAsync(destinationAta,
                Solnet.Rpc.Types.Commitment.Confirmed);

            var txBuilder = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(CurrentWallet.Address);

            if (destinationTokenAccount.WasSuccessful)
            {
                if (destinationTokenAccount.Result.Value == null)
                    txBuilder.AddInstruction(AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                        CurrentWallet.Address,
                        vm.Destination.PublicKey,
                        new(vm.SelectedToken.TokenMint)));
            }
            else
            {
                TransactionSubmission.CraftingError();
                await Task.Delay(15000);
                TransactionSubmission = TransactionSubmissionViewModel.NoShow();
                return;
            }

            var converter = (decimal)Math.Pow(10, vm.SelectedToken.DecimalPlaces);
            var amount = (ulong)(vm.Amount * converter);

            txBuilder.AddInstruction(TokenProgram.Transfer(
                    sourceAta,
                    destinationAta,
                    amount,
                    CurrentWallet.Address));

            var txBytes = txBuilder.CompileMessage();

            var signature = CurrentWallet.Sign(txBytes);

            txBuilder.AddSignature(signature);
            var txSig = await _rpcClient.SendTransactionAsync(txBuilder.Serialize());
            var success = await TransactionSubmission.SubmitTransaction(txBuilder.Serialize());

            if (success)
            {
                await TransactionSubmission.PollConfirmation();
                await GetAccountHoldings();
                await Task.Delay(15000);
                TransactionSubmission = TransactionSubmissionViewModel.NoShow();
            }
            else
            {
                await Task.Delay(15000);
                TransactionSubmission = TransactionSubmissionViewModel.NoShow();
            }
        }

        public async void EditAccountAlias()
        {
            var dialog = DialogHelper.CreateTextFieldDialog(new TextFieldDialogBuilderParams()
            {
                ContentHeader = "Edit Account Alias",
                StartupLocation = WindowStartupLocation.CenterOwner,
                Width = 500,
                Borderless = true,
                TextFields = new TextFieldBuilderParams[]
                {
                    new TextFieldBuilderParams
                    {
                        HelperText = "",
                        Classes = "Outline",
                        Label = "Alias",
                        DefaultText = CurrentWalletAlias,
                        FieldKind = Material.Dialog.Enums.TextFieldKind.Normal
                    },
                },
                DialogButtons = new DialogButton[]
                {
                    new DialogButton()
                    {
                        Content = "Cancel",
                        IsNegative = true,
                        Result = "cancel",
                    }
                }
            });
            var result = await dialog.ShowDialog(_appLifetime.MainWindow);
            if (result.GetResult == "ok")
            {
                var res = result.GetFieldsResult()[0].Text;
                _walletService.EditAlias(CurrentWallet.Address, res);
            }
        }

        /// <summary>
        /// Adds the current wallet's address to the clipboard.
        /// </summary>
        public async void CopyAddressToClipboard()
        {
            await Application.Current.Clipboard.SetTextAsync(CurrentWallet.Address.Key);
        }

        /// <summary>
        /// Requests the wallet service to generate a new wallet from the mnemonic.
        /// </summary>
        public void DeriveWallet() => _walletService.GenerateNewWallet();

        /// <summary>
        /// Mnemonic validation for the dialog.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A tuple with a bool that means whether the input mnemonic was validated and a corresponding validation text.</returns>
        private Tuple<bool, string> ValidateMnemonicDelegate(string text)
        {
            var emptyOrWhiteSpace = string.IsNullOrWhiteSpace(text);
            if (emptyOrWhiteSpace)
                return new Tuple<bool, string>(!emptyOrWhiteSpace, "Empty mnemonic is not valid.");

            Mnemonic mnemonic;
            try
            {
                mnemonic = new Mnemonic(text, WordList.AutoDetect(text));
                if (mnemonic.IsValidChecksum)
                    return new Tuple<bool, string>(mnemonic.IsValidChecksum, mnemonic.IsValidChecksum ? "" : "Mnemonic is invalid.");
            }
            catch (Exception)
            {
                return new Tuple<bool, string>(false, "Mnemonic is invalid.");
            }

            return new Tuple<bool, string>(false, "Mnemonic is invalid.");
        }
        
        /// <summary>
        /// Private key validation for the dialog.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A tuple with a bool that means whether the input mnemonic was validated and a corresponding validation text.</returns>
        private Tuple<bool, string> ValidatePrivateKeyDelegate(string text)
        {
            var emptyOrWhiteSpace = string.IsNullOrWhiteSpace(text);
            if (emptyOrWhiteSpace)
                return new Tuple<bool, string>(!emptyOrWhiteSpace, "Empty private key is not valid.");

            Solnet.Wallet.Wallet wallet;
            try
            {
                wallet = SolanaPrivateKeyLoader.LoadPrivateKey(text);
                if (wallet != null)
                    return new Tuple<bool, string>(true, "Private key is invalid.");
            }
            catch (Exception)
            {
                return new Tuple<bool, string>(false, "Private key is invalid.");
            }

            return new Tuple<bool, string>(false, "Private key is invalid.");
        }

        /// <summary>
        /// Requests the mnemonic from the user.
        /// </summary>
        /// <returns>The mnemonic or null in case the operation is cancelled.</returns>
        private async Task<string> GetMnemonicInfo()
        {
            var dialog = DialogHelper.CreateTextFieldDialog(new TextFieldDialogBuilderParams()
            {
                ContentHeader = "Import Mnemonic",
                StartupLocation = WindowStartupLocation.CenterOwner,
                Width = 500,
                Borderless = true,
                TextFields = new TextFieldBuilderParams[]
                {
                    new TextFieldBuilderParams
                    {
                        HelperText = "* Required",
                        Classes = "Outline",
                        Label = "Mnemonic",
                        Validater = ValidateMnemonicDelegate,
                        DefaultText = "",
                        FieldKind = Material.Dialog.Enums.TextFieldKind.Normal
                    }
                },
                DialogButtons = new DialogButton[]
                {
                    new DialogButton()
                    {
                        Content = "Cancel",
                        IsNegative = true,
                        Result = "cancel",
                    }
                }
            });
            var result = await dialog.ShowDialog(_appLifetime.MainWindow);
            if (result.GetResult == "ok")
            {
                var res = result.GetFieldsResult()[0].Text;
                return res;
            }
            return null;
        }

        /// <summary>
        /// Imports a mnemonic.
        /// </summary>
        public async void ImportMnemonic()
        {
            var mnemonic = await GetMnemonicInfo();
            if (string.IsNullOrWhiteSpace(mnemonic)) return;

            _walletService.AddWallet(mnemonic);
            var ws = _walletService.AddWallet(new DerivationIndexWallet()
            {
                DerivationIndex = 0,
                Alias = "Account 1"
            });
            _walletService.ChangeWallet(ws);
        }

        /// <summary>
        /// Gets a private key file's path from user input.
        /// </summary>
        /// <returns>The path or null in case the operation is cancelled.</returns>
        private async Task<string> GetPrivateKeyFilePath()
        {
            var ofd = new OpenFileDialog()
            {
                AllowMultiple = false,
                Title = "Select Private Key File",
                Filters = new()
                {
                    new FileDialogFilter()
                    {
                        Name = "*",
                        Extensions = new() { "json" }
                    }
                }
            };
            var selected = await ofd.ShowAsync(_appLifetime.MainWindow);
            if (selected == null) return null;
            if (selected.Length > 0)
            {
                return selected.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Imports a private key from file.
        /// </summary>
        public async void ImportPrivateKeyFile()
        {
            var path = await GetPrivateKeyFilePath();
            if (string.IsNullOrWhiteSpace(path)) return;
            if (!File.Exists(path)) return;

            var alias = await ChooseAnAlias();
            if (string.IsNullOrEmpty(alias)) return;

            _walletService.AddWallet(new PrivateKeyWallet
            {
                Alias = alias,
                Path = path
            });
        }

        /// <summary>
        /// Gets a private key file's path from user input.
        /// </summary>
        /// <returns>The path or null in case the operation is cancelled.</returns>
        private async Task<Tuple<string, string>> GetPrivateKey()
        {
            var dialog = DialogHelper.CreateTextFieldDialog(new TextFieldDialogBuilderParams()
            {
                ContentHeader = "Import Private Key",
                StartupLocation = WindowStartupLocation.CenterOwner,
                Width = 500,
                Borderless = true,
                TextFields = new TextFieldBuilderParams[]
                {
                    new TextFieldBuilderParams
                    {
                        HelperText = "* Required",
                        Classes = "Outline",
                        Label = "Private Key",
                        Validater = ValidatePrivateKeyDelegate,
                        DefaultText = "",
                        FieldKind = Material.Dialog.Enums.TextFieldKind.Normal
                    },
                    new TextFieldBuilderParams
                    {
                        Classes = "Outline",
                        Label = "Alias",
                        DefaultText = "",
                        FieldKind = Material.Dialog.Enums.TextFieldKind.Normal
                    }
                },
                DialogButtons = new DialogButton[]
                {
                    new DialogButton()
                    {
                        Content = "Cancel",
                        IsNegative = true,
                        Result = "cancel",
                    }
                }
            });
            var result = await dialog.ShowDialog(_appLifetime.MainWindow);
            if (result.GetResult == "ok")
            {
                var fields = result.GetFieldsResult();

                return new Tuple<string, string>(fields[0].Text, fields[1].Text);
            }
            return null;
        }

        /// <summary>
        /// Imports a private key from file.
        /// </summary>
        public async void ImportPrivateKey()
        {
            var pkInfo = await GetPrivateKey();
            if (pkInfo == null) return;

            var (pk, alias) = pkInfo;
            if (string.IsNullOrWhiteSpace(pk)) return;

            _walletService.AddWallet(new PrivateKeyWallet
            {
                Alias = alias,
                PrivateKey = pk
            });
        }

        /// <summary>
        /// Gets an alias for a private key wallet from user input.
        /// </summary>
        /// <returns>The alias or null in case the operation is cancelled.</returns>
        private async Task<string> ChooseAnAlias()
        {
            var dialog = DialogHelper.CreateTextFieldDialog(new TextFieldDialogBuilderParams()
            {
                ContentHeader = "Choose An Alias",
                SupportingText = "By canceling the wallet will not be imported.",
                StartupLocation = WindowStartupLocation.CenterOwner,
                Width = 500,
                Borderless = true,
                TextFields = new TextFieldBuilderParams[]
            {
                    new TextFieldBuilderParams
                    {
                        Classes = "Outline",
                        Label = "Alias",
                        DefaultText = "",
                        FieldKind = Material.Dialog.Enums.TextFieldKind.Normal
                    },
            },
                DialogButtons = new DialogButton[]
            {
                    new DialogButton()
                    {
                        Content = "Cancel",
                        IsNegative = true,
                        Result = "cancel",
                    }
            }
            });
            var result = await dialog.ShowDialog(_appLifetime.MainWindow);
            if (result.GetResult == "ok")
            {
                var fieldsResult = result.GetFieldsResult();
                var res = fieldsResult.FirstOrDefault();
                if (res == null) return null;
                return res.Text;
            }
            return null;
        }

        /// <summary>
        /// Restores the different wallets and currently selected wallet from the <see cref="IWalletService"/> snapshot.
        /// </summary>
        /// <param name="onlyRefreshLists">Whether to only refresh the <see cref="SubWalletType"/> lists or also the <see cref="CurrentWallet"/>.</param>
        private void RestoreFromWalletSnapshot(bool onlyRefreshLists)
        {
            foreach (var w in _walletService.Wallets)
            {
                switch (w.SubWalletType)
                {
                    case SubWalletType.DerivationIndex:
                        DerivationWalletsCollection.Add(w);
                        break;
                    case SubWalletType.PrivateKey:
                        PrivateKeyWalletsCollection.Add(w);
                        break;
                }
            }
            if (!onlyRefreshLists)
            {
                if (_walletService.CurrentWallet != null)
                {
                    CurrentWallet = _walletService.CurrentWallet;
                    CurrentWalletAlias = _walletService.CurrentWallet.Alias;
                }
            }

            ImportedMnemonic = _walletService.MnemonicImported;
        }

        /// <summary>
        /// Handles a change to the current <see cref="IRpcClient"/> being provided by the <see cref="IRpcClientProvider"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnClientChanged(object sender, Services.Rpc.Events.RpcClientChangedEventArgs e)
        {
            Task.Run(GetAccountHoldings);
        }

        /// <summary>
        /// Handles a change to the <see cref="IWalletService.CurrentWallet"/> being provided.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnCurrentWalletChanged(object sender, Services.Wallets.Events.CurrentWalletChangedEventArgs e)
        {
            if (e.Wallet != CurrentWallet)
            {
                CurrentWallet = e.Wallet;
                CurrentWalletAlias = e.Wallet.Alias;
            }
        }

        /// <summary>
        /// Handles a change to the <see cref="IWalletService"/> state, this could be an addition, removal or change of alias.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnWalletServiceStateChanged(object sender, Services.Events.WalletServiceStateChangedEventArgs e)
        {
            if (e.StateChange == Services.Enums.WalletServiceStateChange.Addition)
            {
                Dispatcher.UIThread.Post(delegate { AddWallet(e.Wallet); });
            }
            else if (e.StateChange == Services.Enums.WalletServiceStateChange.Removal)
            {
                Dispatcher.UIThread.Post(delegate { RemoveWallet(e.Wallet); });
            }
            else if (e.StateChange == Services.Enums.WalletServiceStateChange.AliasChanged)
            {
                Dispatcher.UIThread.Post(delegate { EditWallet(e.Wallet); });
            }
        }

        /// <summary>
        /// Removes a given wallet from the collections.
        /// </summary>
        /// <param name="wallet">The wallet to remove.</param>
        private void RemoveWallet(IWallet wallet)
        {
            switch (wallet.SubWalletType)
            {
                case SubWalletType.DerivationIndex:
                    DerivationWalletsCollection.Remove(wallet);
                    break;
                case SubWalletType.PrivateKey:
                    PrivateKeyWalletsCollection.Remove(wallet);
                    break;
            }
        }

        /// <summary>
        /// Adds a given wallet to the collections.
        /// </summary>
        /// <param name="wallet">The wallet to add.</param>
        private void AddWallet(IWallet wallet)
        {
            switch (wallet.SubWalletType)
            {
                case SubWalletType.DerivationIndex:
                    DerivationWalletsCollection.Add(wallet);
                    break;
                case SubWalletType.PrivateKey:
                    PrivateKeyWalletsCollection.Add(wallet);
                    break;
            }
            ImportedMnemonic = _walletService.MnemonicImported;
        }

        /// <summary>
        /// Edits a given wallet in the collections.
        /// </summary>
        /// <param name="wallet"></param>
        private void EditWallet(IWallet wallet)
        {
            if (wallet == null) return;
            CurrentWalletAlias = _walletService.CurrentWallet.Alias;
            PrivateKeyWalletsCollection = new ObservableCollection<IWallet>();
            DerivationWalletsCollection = new ObservableCollection<IWallet>();

            RestoreFromWalletSnapshot(true);
        }

        /// <summary>
        /// Gets the current account holdings.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        private async Task GetAccountHoldings()
        {
            FetchingBalance = true;
            FetchingTokenBalances = true;

            _resolver ??= await TokenMintResolver.LoadAsync();

            if (CurrentWallet == null)
                await Task.Delay(250);

            await GetAccountBalance();
            FetchingBalance = false;
            await GetTokenBalances();

            FetchingTokenBalances = false;
        }

        /// <summary>
        /// Gets the current account's balance.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        private async Task GetAccountBalance()
        {
            var balance = await _rpcClient.GetBalanceAsync(CurrentWallet.Address, Solnet.Rpc.Types.Commitment.Confirmed);

            if (balance.WasRequestSuccessfullyHandled)
            {
                CurrentNativeBalance = balance.Result.Value;
                CurrentBalance = SolHelper.ConvertToSol(balance.Result.Value);
            }
        }

        /// <summary>
        /// Gets the current account's token balances.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        private async Task GetTokenBalances()
        {
            var tokenWallet = await TokenWallet.LoadAsync(_rpcClient, _resolver, CurrentWallet.Address, Solnet.Rpc.Types.Commitment.Confirmed);

            TokenBalances = new ObservableCollection<TokenWalletBalance>();
            var tokenBalances = tokenWallet.Balances();

            foreach (var tokenBalance in tokenBalances)
            {
                TokenBalances.Add(tokenBalance);
            }

            if (tokenBalances.Length > 0)
                CanSendTokens = true;

            if (tokenBalances.Length == 0)
                CanSendTokens = false;

            UpdateTokenCollection();
        }

        /// <summary>
        /// Updates the token collection being displayed to take into account the search text and/or an NFT filter.
        /// </summary>
        private void UpdateTokenCollection()
        {
            if (HideNfts)
            {
                var balancesWithoutNfts = TokenBalances.Where(x => !(x.DecimalPlaces == 0 && (x.QuantityRaw == 1 || x.QuantityRaw == 0)));
                FilteredTokenBalances = new(balancesWithoutNfts.Where(x =>
                    (x.Symbol?.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                    (x.TokenName?.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                    x.TokenMint.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase)));
            }
            else
            {
                FilteredTokenBalances = new(TokenBalances.Where(x =>
                    (x.Symbol?.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                    (x.TokenName?.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                    x.TokenMint.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        private bool _canSubmitTransaction;
        public bool CanSubmitTransaction
        {
            get => _canSubmitTransaction;
            set => this.RaiseAndSetIfChanged(ref _canSubmitTransaction, value);
        }

        private TransactionSubmissionViewModel _transactionSubmission;
        public TransactionSubmissionViewModel TransactionSubmission
        {
            get => _transactionSubmission;
            set => this.RaiseAndSetIfChanged(ref _transactionSubmission, value);
        }

        private bool _importedMnemonic;
        public bool ImportedMnemonic
        {
            get => _importedMnemonic;
            set => this.RaiseAndSetIfChanged(ref _importedMnemonic, value);
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                this.RaiseAndSetIfChanged(ref _searchText, value);
                UpdateTokenCollection();
            }
        }

        private bool _fetchingBalance;
        public bool FetchingBalance
        {
            get => _fetchingBalance;
            set => this.RaiseAndSetIfChanged(ref _fetchingBalance, value);
        }

        private bool _noConnection;
        public bool NoConnection
        {
            get => _noConnection;
            set => this.RaiseAndSetIfChanged(ref _noConnection, value);
        }

        private bool _hideNfts;
        public bool HideNfts
        {
            get => _hideNfts;
            set
            {
                this.RaiseAndSetIfChanged(ref _hideNfts, value);
                UpdateTokenCollection();
            }
        }

        private bool _fetchingTokenBalances;
        public bool FetchingTokenBalances
        {
            get => _fetchingTokenBalances;
            set => this.RaiseAndSetIfChanged(ref _fetchingTokenBalances, value);
        }

        private decimal _currentBalance;
        public decimal CurrentBalance
        {
            get => _currentBalance;
            set => this.RaiseAndSetIfChanged(ref _currentBalance, value);
        }

        private ulong _currentNativeBalance;
        public ulong CurrentNativeBalance
        {
            get => _currentNativeBalance;
            set => this.RaiseAndSetIfChanged(ref _currentNativeBalance, value);
        }

        private string _currentWalletAlias;
        public string CurrentWalletAlias
        {
            get => _currentWalletAlias;
            set => this.RaiseAndSetIfChanged(ref _currentWalletAlias, value);
        }

        private IWallet _currentWallet;
        public IWallet CurrentWallet
        {
            get => _currentWallet;
            set
            {
                if (value != null)
                {
                    if (value != _walletService.CurrentWallet)
                    {
                        Task.Run(delegate { _walletService.ChangeWallet(value); });
                        return;
                    }
                    else
                    {
                        this.RaiseAndSetIfChanged(ref _currentWallet, value);
                        if (NoConnection) return;
                        Task.Run(GetAccountHoldings);
                    }
                }
            }
        }

        private ObservableCollection<TokenWalletBalance> _tokenBalances;
        public ObservableCollection<TokenWalletBalance> TokenBalances
        {
            get => _tokenBalances;
            set => this.RaiseAndSetIfChanged(ref _tokenBalances, value);
        }

        private ObservableCollection<TokenWalletBalance> _filteredTokenBalances;
        public ObservableCollection<TokenWalletBalance> FilteredTokenBalances
        {
            get => _filteredTokenBalances;
            set => this.RaiseAndSetIfChanged(ref _filteredTokenBalances, value);
        }

        private ObservableCollection<IWallet> _drvwCollection;
        public ObservableCollection<IWallet> DerivationWalletsCollection
        {
            get => _drvwCollection;
            set => this.RaiseAndSetIfChanged(ref _drvwCollection, value);
        }

        private ObservableCollection<IWallet> _pkwCollection;
        public ObservableCollection<IWallet> PrivateKeyWalletsCollection
        {
            get => _pkwCollection;
            set => this.RaiseAndSetIfChanged(ref _pkwCollection, value);
        }

        private bool _canSendTokens;
        public bool CanSendTokens
        {
            get => _canSendTokens;
            set => this.RaiseAndSetIfChanged(ref _canSendTokens, value);
        }
    }
}
