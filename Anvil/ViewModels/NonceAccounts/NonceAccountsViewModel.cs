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
using Solnet.Programs.Models;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Wallet;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Anvil.ViewModels.NonceAccounts
{
    public class NonceAccountsViewModel : ViewModelBase
    {
        private IClassicDesktopStyleApplicationLifetime _appLifetime;

        private IRpcClientProvider _rpcProvider;
        private IRpcClient _rpcClient => _rpcProvider.Client;
        private IWalletService _walletService;
        private INonceAccountMappingStore _nonceAccountMappingStore;

        private AddressBookService _addressBookService;
        private InternetConnectionService _internetConnectionService;


        public NonceAccountsViewModel(IClassicDesktopStyleApplicationLifetime appLifetime,
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
            _nonceAccountMappingStore.OnStateChanged += OnStateChanged;
            _addressBookService = addressBookService;

            NonceAccounts = new();
            NoConnection = !_internetConnectionService.IsConnected;
            TransactionSubmission = TransactionSubmissionViewModel.NoShow();

            HandleStoreSnapshot();
        }

        private void OnNetworkConnectionChanged(object? sender, Services.Network.Events.NetworkConnectionChangedEventArgs e)
        {
            NoConnection = !e.Connected;
        }

        private void OnStateChanged(object? sender, Services.Store.Events.NonceAccountMappingStateChangedEventArgs e)
        {
            HandleStoreSnapshot();
        }

        public async void CreateNonceAccount()
        {
            var vm = await GetNonceAccountInfo();
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
                    NonceAccount.AccountDataSize,
                    SystemProgram.ProgramIdKey
                    ))
                .AddInstruction(SystemProgram.InitializeNonceAccount(
                    vm.Account,
                    vm.Authority.PublicKey));

            var msgBytes = txBuilder.CompileMessage();

            txBuilder.AddSignature(_walletService.CurrentWallet.Sign(msgBytes));
            txBuilder.AddSignature(vm.Account.Sign(msgBytes));

            // submit create nonce and poll confirmation

            var success = await TransactionSubmission.SubmitTransaction(txBuilder.Serialize());

            if (success)
            {
                await TransactionSubmission.PollConfirmation();
                // once confirmed add to local storage
                _nonceAccountMappingStore.AddMapping(new NonceAccountMapping
                {
                    Account = vm.Account.PublicKey,
                    Authority = vm.Authority.PublicKey
                });
                await Task.Delay(15000);
                TransactionSubmission = TransactionSubmissionViewModel.NoShow();
            }
            else
            {
                await Task.Delay(15000);
                TransactionSubmission = TransactionSubmissionViewModel.NoShow();
            }
        }

        private async Task<CreateNonceAccountDialogViewModel?> GetNonceAccountInfo()
        {
            var rent = await _rpcClient.GetMinimumBalanceForRentExemptionAsync(NonceAccount.AccountDataSize);

            var vm = new CreateNonceAccountDialogViewModel()
            {
                NativeRent = rent.Result,
                AddressBookService = _addressBookService
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

        public async void ImportNonceAccount()
        {
            var ofd = new OpenFileDialog()
            {
                AllowMultiple = false,
                Title = "Select Nonce Account Key File",
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

                var nonceAccount = await GetNonceAccount(accountKey.Account.PublicKey);

                if (nonceAccount != null)
                {
                    var mapping = new NonceAccountMapping
                    {
                        Account = accountKey.Account.PublicKey,
                        Authority = nonceAccount.Authorized
                    };
                    // once confirmed add to local storage
                    _nonceAccountMappingStore.AddMapping(mapping);
                    CurrentNonceAccountMapping = mapping;
                }
                else
                {
                    var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
                    {
                        ContentHeader = "Error Loading Account Data",
                        SupportingText =
                        "The data of the specified account could not be loaded.\n" +
                        "Are you sure it is a Nonce Account?",
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
                    ErrorFetchingNonceAccount = false;
                }
            }
        }

        private void HandleStoreSnapshot()
        {
            NonceAccounts = new();

            foreach (var item in _nonceAccountMappingStore.NonceAccountMappings)
            {
                NonceAccounts.Add(item);
            }

            if (NonceAccounts.Count == 0)
            {
                NoAccountsFound = true;
                return;
            }

            if (CurrentNonceAccountMapping == null)
                CurrentNonceAccountMapping = NonceAccounts.First();
        }

        private async Task<NonceAccount?> GetNonceAccount(string? account = null)
        {
            FetchingNonceAccount = true;
            ErrorFetchingNonceAccount = false;

            string requestKey = account != null ? account : CurrentNonceAccountMapping != null ? CurrentNonceAccountMapping.Account : string.Empty;

            if (string.IsNullOrEmpty(requestKey)) return null;

            var nonceAccountInfo = await _rpcClient.GetAccountInfoAsync(requestKey, Solnet.Rpc.Types.Commitment.Confirmed);
            if (nonceAccountInfo.WasSuccessful)
            {
                if (nonceAccountInfo.Result.Value == null)
                {
                    // trigger error
                    FetchingNonceAccount = false;
                    CurrentNonce = string.Empty;
                    ErrorFetchingNonceAccount = true;
                    return null;
                }
                byte[] accountDataBytes = Convert.FromBase64String(nonceAccountInfo.Result.Value.Data[0]);

                if (accountDataBytes.Length != NonceAccount.AccountDataSize)
                {
                    FetchingNonceAccount = false;
                    CurrentNonce = string.Empty;
                    ErrorFetchingNonceAccount = true;
                    return null;
                }

                NonceAccount nonceAccount = NonceAccount.Deserialize(accountDataBytes);

                if (account != null)
                {
                    CurrentNonceAccount = nonceAccount;
                    CurrentNonce = CurrentNonceAccount.Nonce.Key;
                    FetchingNonceAccount = false;
                    return nonceAccount;
                }

                return null;
            }

            // trigger error
            FetchingNonceAccount = false;
            ErrorFetchingNonceAccount = true;
            return null;
        }

        private TransactionSubmissionViewModel _transactionSubmission;
        public TransactionSubmissionViewModel TransactionSubmission
        {
            get => _transactionSubmission;
            set => this.RaiseAndSetIfChanged(ref _transactionSubmission, value);
        }

        private bool _fetchingNonceAccount;
        public bool FetchingNonceAccount
        {
            get => _fetchingNonceAccount;
            set => this.RaiseAndSetIfChanged(ref _fetchingNonceAccount, value);
        }

        private bool _noAccountsFound;
        public bool NoAccountsFound
        {
            get => _noAccountsFound;
            set => this.RaiseAndSetIfChanged(ref _noAccountsFound, value);
        }

        private bool _errorFetchingNonceAccount;
        public bool ErrorFetchingNonceAccount
        {
            get => _errorFetchingNonceAccount;
            set => this.RaiseAndSetIfChanged(ref _errorFetchingNonceAccount, value);
        }

        private NonceAccountMapping _currentNonceAccountMapping;
        public NonceAccountMapping CurrentNonceAccountMapping
        {
            get => _currentNonceAccountMapping;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentNonceAccountMapping, value);
                if (NoAccountsFound) NoAccountsFound = false;
                if (NoConnection) return;
                Task.Run(async () =>
                {
                    if (_currentNonceAccountMapping == null) return;
                    var nonce = GetNonceAccount(_currentNonceAccountMapping.Account);
                    if (nonce == null)
                    {
                        await Task.Delay(5000);
                        ErrorFetchingNonceAccount = false;
                    }
                });
            }
        }

        private bool _noConnection;
        public bool NoConnection
        {
            get => _noConnection;
            set => this.RaiseAndSetIfChanged(ref _noConnection, value);
        }

        private string _currentNonce;
        public string CurrentNonce
        {
            get => _currentNonce;
            set => this.RaiseAndSetIfChanged(ref _currentNonce, value);
        }

        private NonceAccount _currentNonceAccount;
        public NonceAccount CurrentNonceAccount
        {
            get => _currentNonceAccount;
            set => this.RaiseAndSetIfChanged(ref _currentNonceAccount, value);
        }

        private ObservableCollection<NonceAccountMapping> _nonceAccounts;
        public ObservableCollection<NonceAccountMapping> NonceAccounts
        {
            get => _nonceAccounts;
            set => this.RaiseAndSetIfChanged(ref _nonceAccounts, value);
        }
    }
}
