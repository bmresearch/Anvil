using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.Services.Network;
using Anvil.Services.Store.Models;
using Anvil.ViewModels.Common;
using Anvil.ViewModels.Dialogs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Material.Dialog;
using ReactiveUI;
using Solnet.Programs;
using Solnet.Programs.Models.TokenProgram;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Wallet;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Anvil.ViewModels.MultiSignatures
{
    public class MultiSignaturesViewModel : ViewModelBase
    {
        private IClassicDesktopStyleApplicationLifetime _appLifetime;

        private IRpcClientProvider _rpcProvider;
        private IRpcClient _rpcClient => _rpcProvider.Client;
        private IWalletService _walletService;
        private IMultiSignatureAccountMappingStore _multiSigAccountMappingStore;

        private AddressBookService _addressBookService;
        private InternetConnectionService _internetConnectionService;

        public MultiSignaturesViewModel(IClassicDesktopStyleApplicationLifetime appLifetime,
            InternetConnectionService internetConnectionService,
            IRpcClientProvider rpcProvider, IWalletService walletService,
            IMultiSignatureAccountMappingStore multiSignatureAccountMappingStore, AddressBookService addressBookService)
        {
            _appLifetime = appLifetime;
            _internetConnectionService = internetConnectionService;
            _internetConnectionService.OnNetworkConnectionChanged += OnNetworkConnectionChanged;
            _rpcProvider = rpcProvider;
            _walletService = walletService;
            _multiSigAccountMappingStore = multiSignatureAccountMappingStore;
            _multiSigAccountMappingStore.OnStateChanged += _multiSigAccountMappingStore_OnStateChanged;
            _addressBookService = addressBookService;

            NoConnection = !_internetConnectionService.IsConnected;
            TransactionSubmission = TransactionSubmissionViewModel.NoShow();

            HandleStoreSnapshot();
        }

        private void OnNetworkConnectionChanged(object? sender, Services.Network.Events.NetworkConnectionChangedEventArgs e)
        {
            NoConnection = !e.Connected;
        }

        private void _multiSigAccountMappingStore_OnStateChanged(object? sender, Services.Store.Events.MultiSignatureAccountMappingStateChangedEventArgs e)
        {
            HandleStoreSnapshot();
        }

        /// <summary>
        /// Gets the multi signature account info necessary to create it from user input.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        private async Task<CreateMultiSignatureAccountDialogViewModel?> GetMultiSignatureAccountInfo()
        {
            var rent = await _rpcClient.GetMinimumBalanceForRentExemptionAsync(MultiSignatureAccount.Layout.Length);

            var vm = new CreateMultiSignatureAccountDialogViewModel()
            {
                NativeRent = rent.Result,
                AddressBookService = _addressBookService,
                Signers = new ObservableCollection<Fields.RequiredPublicKeyViewModel>()
                {
                    new Fields.RequiredPublicKeyViewModel(true, _addressBookService),
                    new Fields.RequiredPublicKeyViewModel(true, _addressBookService),
                }
            };

            var dialog = DialogHelper.CreateCustomDialog(new CustomDialogBuilderParams
            {
                Borderless = true,
                Content = vm,
                StartupLocation = WindowStartupLocation.CenterOwner,
                Width = 600,
            });
            await dialog.ShowDialog(_appLifetime.MainWindow);
            if (!vm.Confirmed) return null;
            return vm;
        }

        public async void ImportMultiSignatureAccount()
        {
            var ofd = new OpenFileDialog()
            {
                AllowMultiple = false,
                Title = "Select Multisig Account Key File",
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
            if (selected == null) return;
            if (selected.Length > 0)
            {
                var accountKey = SolanaPrivateKeyLoader.Load(selected[0]);
                var alias = await ChooseAnAlias();

                await ConfirmAndAddToStorageOrThrowError(accountKey.Account, alias);
            }
        }

        /// <summary>
        /// Validation for the dialog.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A tuple with a bool that means whether the input public key was validated and a corresponding validation text.</returns>
        private Tuple<bool, string> ValidatePublicKeyDelegate(string text)
        {
            var emptyOrWhiteSpace = string.IsNullOrWhiteSpace(text);
            if (emptyOrWhiteSpace)
                return new Tuple<bool, string>(!emptyOrWhiteSpace, "Empty public key is not valid.");

            PublicKey pk;
            try
            {
                pk = new PublicKey(text);
                return new Tuple<bool, string>(pk != null, pk != null ? "" : "Public key is invalid.");
            }
            catch (Exception)
            {
                return new Tuple<bool, string>(false, "Public key is invalid.");
            }
        }

        public async Task ImportMultiSignatureAccountFromPublicKey()
        {
            var dialog = DialogHelper.CreateTextFieldDialog(new TextFieldDialogBuilderParams()
            {
                ContentHeader = "Import Multisig Account",
                SupportingText = "By canceling the account will not be imported.",
                StartupLocation = WindowStartupLocation.CenterOwner,
                Width = 500,
                Borderless = true,
                TextFields = new TextFieldBuilderParams[]
                {
                    new TextFieldBuilderParams
                    {
                        HelperText = "* Required",
                        Classes = "Outline",
                        Label = "Public Key",
                        Validater = ValidatePublicKeyDelegate,
                        DefaultText = "",
                        FieldKind = Material.Dialog.Enums.TextFieldKind.Normal
                    },
                    new TextFieldBuilderParams
                    {
                        HelperText = "",
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
                var account = fieldsResult[0];
                var alias = fieldsResult[1];

                await ConfirmAndAddToStorageOrThrowError(new(account.Text), alias.Text);
            }
        }

        /// <summary>
        /// Gets an alias for a private key wallet from user input.
        /// </summary>
        /// <returns>The alias or null in case the operation is cancelled.</returns>
        private async Task<string?> ChooseAnAlias()
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

        private async Task ConfirmAndAddToStorageOrThrowError(PublicKey account, string alias)
        {
            var multisig = await GetMultiSignatureAccount(account);

            if (multisig != null)
            {
                // once confirmed add to local storage
                var mapping = new MultiSignatureAccountMapping
                {
                    Alias = alias,
                    Address = account,
                    MinimumSigners = multisig.MinimumSigners,
                    Signers = multisig.Signers.Select(x => x.Key).ToList(),
                };
                _multiSigAccountMappingStore.AddMapping(mapping);
                CurrentMultiSigAccountMapping = mapping;
            }
            else
            {
                var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
                {
                    ContentHeader = "Error Loading Account Data",
                    SupportingText =
                    "The data of the specified account could not be loaded.\n" +
                    "Are you sure it is a Multisig Account?",
                    DialogIcon = Material.Dialog.Icons.DialogIconKind.Warning,
                    StartupLocation = WindowStartupLocation.CenterOwner,
                    Width = 500,
                    Borderless = true,
                });
                var result = await dialog.ShowDialog(_appLifetime.MainWindow);
                if (result.GetResult == "ok")
                {
                    return;
                }

                await Task.Delay(5000);
                ErrorFetchingMultiSigAccount = false;
            }
        }

        public async void CreateMultiSignatureAccount()
        {
            var vm = await GetMultiSignatureAccountInfo();
            if (vm == null) return;

            TransactionSubmission = new(_rpcProvider);

            var blockHash = await _rpcClient.GetRecentBlockHashAsync();

            var txBuilder = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(_walletService.CurrentWallet.Address)
                .AddInstruction(SystemProgram.CreateAccount(
                    _walletService.CurrentWallet.Address,
                    vm.Account,
                    vm.NativeRent,
                    MultiSignatureAccount.Layout.Length,
                    TokenProgram.ProgramIdKey))
                .AddInstruction(TokenProgram.InitializeMultiSignature(
                    vm.Account,
                    vm.Signers.Select(x => x.PublicKey),
                    vm.MinimumSigners));

            var msgBytes = txBuilder.CompileMessage();

            txBuilder.AddSignature(_walletService.CurrentWallet.Sign(msgBytes));
            txBuilder.AddSignature(vm.Account.Sign(msgBytes));

            var success = await TransactionSubmission.SubmitTransaction(txBuilder.Serialize());

            if (success)
            {
                await TransactionSubmission.PollConfirmation();
                await Task.Delay(15000);
                TransactionSubmission = TransactionSubmissionViewModel.NoShow();
            }
            else
            {
                await Task.Delay(15000);
                TransactionSubmission = TransactionSubmissionViewModel.NoShow();
            }
        }

        private void HandleStoreSnapshot()
        {
            MultiSigAccounts = new();

            foreach (var item in _multiSigAccountMappingStore.MultiSignatureAccountMappings)
            {
                MultiSigAccounts.Add(item);
            }

            if (MultiSigAccounts.Count == 0)
            {
                NoAccountsFound = true;
                return;
            }

            if (CurrentMultiSigAccountMapping == null)
                CurrentMultiSigAccountMapping = MultiSigAccounts.FirstOrDefault();
        }

        /// <summary>
        /// Gets the multi signature account info from the chain.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private async Task<MultiSignatureAccount?> GetMultiSignatureAccount(string? account = null)
        {
            FetchingMultiSigAccount = true;
            ErrorFetchingMultiSigAccount = false;

            var requestKey = account != null ? account : string.Empty;

            if (requestKey == null) return null;

            var nonceAccountInfo = await _rpcClient.GetAccountInfoAsync(requestKey, Solnet.Rpc.Types.Commitment.Confirmed);
            if (nonceAccountInfo.WasSuccessful)
            {
                if (nonceAccountInfo.Result.Value == null)
                {
                    // trigger error
                    FetchingMultiSigAccount = false;
                    ErrorFetchingMultiSigAccount = true;
                    return null;
                }
                byte[] accountDataBytes = Convert.FromBase64String(nonceAccountInfo.Result.Value.Data[0]);

                if (accountDataBytes.Length != MultiSignatureAccount.Layout.Length)
                {
                    // trigger error
                    FetchingMultiSigAccount = false;
                    ErrorFetchingMultiSigAccount = true;
                    return null;
                }

                MultiSignatureAccount multiSigAccount = MultiSignatureAccount.Deserialize(accountDataBytes);

                if (account != null)
                {
                    CurrentMultiSigAccount = multiSigAccount;
                    FetchingMultiSigAccount = false;
                    ErrorFetchingMultiSigAccount = false;
                    return multiSigAccount;
                }

                return null;
            }

            // trigger error
            FetchingMultiSigAccount = false;
            ErrorFetchingMultiSigAccount = true;
            return null;
        }

        private TransactionSubmissionViewModel _transactionSubmission;
        public TransactionSubmissionViewModel TransactionSubmission
        {
            get => _transactionSubmission;
            set => this.RaiseAndSetIfChanged(ref _transactionSubmission, value);
        }

        private bool _submittingTransaction;
        public bool SubmittingTransaction
        {
            get => _submittingTransaction;
            set => this.RaiseAndSetIfChanged(ref _submittingTransaction, value);
        }

        private bool _noConnection;
        public bool NoConnection
        {
            get => _noConnection;
            set => this.RaiseAndSetIfChanged(ref _noConnection, value);
        }

        private bool _noAccountsFound;
        public bool NoAccountsFound
        {
            get => _noAccountsFound;
            set => this.RaiseAndSetIfChanged(ref _noAccountsFound, value);
        }

        private bool _transactionError;
        public bool TransactionError
        {
            get => _transactionError;
            set => this.RaiseAndSetIfChanged(ref _transactionError, value);
        }

        private bool _transactionConfirmed;
        public bool TransactionConfirmed
        {
            get => _transactionConfirmed;
            set => this.RaiseAndSetIfChanged(ref _transactionConfirmed, value);
        }

        private string _transactionHash;
        public string TransactionHash
        {
            get => _transactionHash;
            set => this.RaiseAndSetIfChanged(ref _transactionHash, value);
        }

        private string _progress;
        public string Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private string _transactionErrorMessage;
        public string TransactionErrorMessage
        {
            get => _transactionErrorMessage;
            set => this.RaiseAndSetIfChanged(ref _transactionErrorMessage, value);
        }

        private bool _fetchingMultiSigAccount;
        public bool FetchingMultiSigAccount
        {
            get => _fetchingMultiSigAccount;
            set => this.RaiseAndSetIfChanged(ref _fetchingMultiSigAccount, value);
        }

        private bool _errorFetchingMultiSigAccount;
        public bool ErrorFetchingMultiSigAccount
        {
            get => _errorFetchingMultiSigAccount;
            set => this.RaiseAndSetIfChanged(ref _errorFetchingMultiSigAccount, value);
        }

        private MultiSignatureAccountMapping? _currentMultiSigAccountMapping;
        public MultiSignatureAccountMapping? CurrentMultiSigAccountMapping
        {
            get => _currentMultiSigAccountMapping;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentMultiSigAccountMapping, value);
                if (NoAccountsFound) NoAccountsFound = false;
                if (NoConnection) return;
                Task.Run(async () =>
                {
                    if (_currentMultiSigAccountMapping == null) return;
                    var multisig = GetMultiSignatureAccount(_currentMultiSigAccountMapping.Address);
                    if (multisig == null)
                    {
                        await Task.Delay(5000);
                        ErrorFetchingMultiSigAccount = false;
                    }
                });
            }
        }

        private MultiSignatureAccount? _currentMultiSigAccount;
        public MultiSignatureAccount? CurrentMultiSigAccount
        {
            get => _currentMultiSigAccount;
            set => this.RaiseAndSetIfChanged(ref _currentMultiSigAccount, value);
        }

        private ObservableCollection<MultiSignatureAccountMapping> _multiSigAccounts;
        public ObservableCollection<MultiSignatureAccountMapping> MultiSigAccounts
        {
            get => _multiSigAccounts;
            set => this.RaiseAndSetIfChanged(ref _multiSigAccounts, value);
        }
    }
}
