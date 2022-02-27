using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.Services.Network;
using Anvil.Services.Store.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Material.Dialog;
using ReactiveUI;
using Solnet.Extensions;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Wallet;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Anvil.ViewModels.WatchOnly
{
    public class WatchOnlyViewModel : ViewModelBase
    {
        private IClassicDesktopStyleApplicationLifetime _appLifetime;
        private IRpcClientProvider _rpcClientProvider;
        private IRpcClient _rpcClient => _rpcClientProvider.Client;
        private IWatchOnlyAccountStore _accountStore;
        private ITokenMintResolver _resolver;
        private InternetConnectionService _internetConnectionService;

        public WatchOnlyViewModel(IClassicDesktopStyleApplicationLifetime appLifetime,
            InternetConnectionService internetConnectionService,
            IRpcClientProvider rpcClientProvider, IWatchOnlyAccountStore watchOnlyAccountStore)
        {
            _appLifetime = appLifetime;
            _rpcClientProvider = rpcClientProvider;
            _rpcClientProvider.OnClientChanged += OnClientChanged;
            _internetConnectionService = internetConnectionService;
            _internetConnectionService.OnNetworkConnectionChanged += OnNetworkConnectionChanged;
            _accountStore = watchOnlyAccountStore;

            NoConnection = !_internetConnectionService.IsConnected;

            HandleStoreSnapshot();
        }

        private void OnNetworkConnectionChanged(object sender, Services.Network.Events.NetworkConnectionChangedEventArgs e)
        {
            NoConnection = !e.Connected;
        }

        private void OnClientChanged(object sender, Services.Rpc.Events.RpcClientChangedEventArgs e)
        {
            Task.Run(GetAccountHoldings);
        }

        public async Task GetAccountHoldings()
        {
            FetchingBalance = true;
            FetchingTokenBalances = true;

            _resolver ??= await TokenMintResolver.LoadAsync();

            if (CurrentAccount == null)
                await Task.Delay(250);

            await GetAccountBalance();
            FetchingBalance = false;
            await GetTokenBalances();

            FetchingTokenBalances = false;
        }

        private async Task GetAccountBalance()
        {
            if (CurrentAccount == null) return;
            var balance = await _rpcClient.GetBalanceAsync(CurrentAccount.Address);

            if (balance.WasRequestSuccessfullyHandled)
                CurrentBalance = SolHelper.ConvertToSol(balance.Result.Value);
        }

        private async Task GetTokenBalances()
        {
            if (CurrentAccount == null) return;
            var tokenWallet = await TokenWallet.LoadAsync(_rpcClient, _resolver, new(CurrentAccount.Address));

            TokenBalances = new ObservableCollection<TokenWalletBalance>();
            var tokenBalances = tokenWallet.Balances();

            foreach (var tokenBalance in tokenBalances)
            {
                TokenBalances.Add(tokenBalance);
            }

            UpdateTokenCollection();
        }

        private void HandleStoreSnapshot()
        {
            var current = CurrentAccount != null ? "" + CurrentAccount.Address : null;

            WatchOnlyAccounts = new ObservableCollection<WatchOnlyAccount>();

            foreach (var item in _accountStore.WatchOnlyAccounts)
            {
                WatchOnlyAccounts.Add(item);
            }

            if (WatchOnlyAccounts.Count == 0)
            {
                NoAccountsFound = true;
                return;
            }

            if (current != null)
            {
                // current account is set so try to maintain it
                var first = WatchOnlyAccounts.FirstOrDefault(x => x.Address == current);
                if (first != null)
                    CurrentAccount = first;
            }
            else
            {
                // current account isn't set so just use the first one
                var first = WatchOnlyAccounts.FirstOrDefault();
                if (first != null)
                    CurrentAccount = first;
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
                var res = result.GetFieldsResult()[0].Text;
                _accountStore.EditAlias(CurrentAccount.Address, res);

                HandleStoreSnapshot();
            }
        }

        public void CopyAddressToClipboard()
        {
            Application.Current.Clipboard.SetTextAsync(CurrentAccount.Address);
        }

        public async void AddWatchOnlyAccount()
        {
            var dialog = DialogHelper.CreateTextFieldDialog(new TextFieldDialogBuilderParams()
            {
                ContentHeader = "Add Watch Only Account",
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
                var watchOnly = new WatchOnlyAccount
                {
                    Address = result.GetFieldsResult()[0].Text,
                    Alias = result.GetFieldsResult()[1].Text
                };
                _accountStore.AddAccount(watchOnly);
                HandleStoreSnapshot();
            }
        }

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

        private bool _noConnection;
        public bool NoConnection
        {
            get => _noConnection;
            set => this.RaiseAndSetIfChanged(ref _noConnection, value);
        }

        private decimal _currentBalance;
        public decimal CurrentBalance
        {
            get => _currentBalance;
            set => this.RaiseAndSetIfChanged(ref _currentBalance, value);
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

        private bool _noAccountsFound;
        public bool NoAccountsFound
        {
            get => _noAccountsFound;
            set => this.RaiseAndSetIfChanged(ref _noAccountsFound, value);
        }

        private bool _fetchingBalance;
        public bool FetchingBalance
        {
            get => _fetchingBalance;
            set => this.RaiseAndSetIfChanged(ref _fetchingBalance, value);
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

        private WatchOnlyAccount _currentWatchOnlyAccount;
        public WatchOnlyAccount CurrentAccount
        {
            get => _currentWatchOnlyAccount;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentWatchOnlyAccount, value);
                if (NoAccountsFound) NoAccountsFound = false;
                if (NoConnection) return;
                Task.Run(GetAccountHoldings);
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

        private ObservableCollection<WatchOnlyAccount> _watchOnlyAccounts;
        public ObservableCollection<WatchOnlyAccount> WatchOnlyAccounts
        {
            get => _watchOnlyAccounts;
            set => this.RaiseAndSetIfChanged(ref _watchOnlyAccounts, value);
        }

        public bool CanSendTokens { get => false; }
    }
}
