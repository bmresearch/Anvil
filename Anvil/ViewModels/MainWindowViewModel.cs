
using Anvil.Services;
using Anvil.Services.Network;
using Anvil.Services.Wallets;
using System;
using ReactiveUI;
using Anvil.Services.Network.Events;
using Anvil.Core.ViewModels;
using Anvil.ViewModels.Crafter;
using Anvil.ViewModels.Wallet;
using Anvil.Models;
using Anvil.Services.Store;
using Anvil.ViewModels.MultiSignatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Anvil.Services.Store.Config;
using System.Threading.Tasks;
using Anvil.ViewModels.WatchOnly;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Material.Dialog;
using Avalonia.Controls;
using Anvil.ViewModels.NonceAccounts;
using System.Windows.Input;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Anvil.Services.Wallets.Enums;
using Anvil.Services.Enums;
using System.Linq;

namespace Anvil.ViewModels
{
    /// <summary>
    /// The main window view model.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private ILogger _logger;

        #region Framework 

        private IAssetLoader _assetLoader;
        private IClassicDesktopStyleApplicationLifetime _appLifetime;
        private IAvaloniaDependencyResolver _resolver;

        #endregion

        #region Application Services And Modules

        private IKeyStore _keyStore;
        private IWalletService _walletService;
        private INonceAccountMappingStore _nonceAccountMappingStore;
        private IMultiSignatureAccountMappingStore _multisigAccountMappingStore;
        private IWatchOnlyAccountStore _watchOnlyAccountStore;
        private IRpcClientProvider _rpcProvider;

        private ApplicationState _appState;
        private InternetConnectionService _internetService;
        private KeyStoreService _keyStoreService;
        private AddressBookService _addressBookService;

        #endregion

        #region View Models

        private CrafterViewModel _crafterViewModel;
        private MultiSignaturesViewModel _multiSigsViewModel;
        private NonceAccountsViewModel _nonceAccountsViewModel;
        private WalletViewModel _walletViewModel;
        private ImportWalletViewModel _importWalletViewModel;
        private UnlockWalletViewModel _unlockWalletViewModel;
        private SettingsViewModel _settingsViewModel;
        private WatchOnlyViewModel _watchOnlyViewModel;

        #endregion

        /// <summary>
        /// Initializes the main window view model.
        /// </summary>
        /// <param name="appLifetime">The application lifetime.</param>
        /// <param name="appState">The application state.</param>
        /// <param name="logger">The logger instance.</param>
        public MainWindowViewModel(IAvaloniaDependencyResolver resolver, IClassicDesktopStyleApplicationLifetime appLifetime,
            ApplicationState appState, ILogger logger)
        {
            // TODO: Add support for the app to stay in the tray
            //using var image = _assetLoader.Open(new("avares://Anvil/Assets/anvil.png"));
            //Bitmap bitmap = new Bitmap(image);
            //TrayIcon = new WindowIcon(bitmap);

            _appLifetime = appLifetime;
            _resolver = resolver;
            _logger = logger;
            _logger.Log(LogLevel.Information, "Successfully attached logger, initializing modules.");
            _assetLoader = _resolver.GetService<IAssetLoader>();
            _appState = appState;

            Wallets = new();

            InitializeServices();

            InitializeView();
        }

        /// <summary>
        /// Initialize the necessary services.
        /// TODO: Add autofac for easier expansion
        /// </summary>
        private void InitializeServices()
        {
            _rpcProvider = _appState.RpcUrl != string.Empty ?
                new RpcClientProvider(_appState.RpcUrl) : new RpcClientProvider(_appState.Cluster);

            // the nonce account mapping store
            var nonceAccountMappingStoreConfig = new StoreConfig()
            {
                Directory = _appState.StorePath,
                Name = NonceAccountMappingStore.FileName
            };
            _nonceAccountMappingStore = new NonceAccountMappingStore(_logger, nonceAccountMappingStoreConfig);
            _logger.Log(LogLevel.Information, "Initialized nonce accounts store.");

            // the multisig account mapping store
            var multisigAccountMappingStoreConfig = new StoreConfig()
            {
                Directory = _appState.StorePath,
                Name = MultiSignatureAccountMappingStore.FileName
            };
            _multisigAccountMappingStore = new MultiSignatureAccountMappingStore(_logger, multisigAccountMappingStoreConfig);
            _logger.Log(LogLevel.Information, "Initialized multisig accounts store.");

            // the watch-only account store
            var watchOnlyAccountStoreConfig = new StoreConfig()
            {
                Directory = _appState.StorePath,
                Name = WatchOnlyAccountStore.FileName
            };
            _watchOnlyAccountStore = new WatchOnlyAccountStore(_logger, watchOnlyAccountStoreConfig);
            _logger.Log(LogLevel.Information, "Initialized watch-only accounts store.");

            // the actual key store
            var keyStoreConfig = new StoreConfig()
            {
                Directory = _appState.StorePath,
                Name = KeyStore.FileName
            };
            _keyStore = new KeyStore(_logger, keyStoreConfig);
            _logger.Log(LogLevel.Information, "Initialized key store.");

            // the services
            _walletService = new WalletService(_keyStore);
            _walletService.OnCurrentWalletChanged += _walletService_OnCurrentWalletChanged;
            _walletService.OnWalletServiceStateChanged += _walletService_OnWalletServiceStateChanged;
            _keyStoreService = new KeyStoreService(_logger, _walletService, _keyStore);
            _keyStoreService.OnStartupStateChanged += _keyStoreService_OnStartupStateChanged;
            _keyStoreService.OnLoadingError += _keyStoreService_OnLoadingError;

            _addressBookService = new AddressBookService(_walletService,
                _multisigAccountMappingStore, _watchOnlyAccountStore);

            _internetService = new InternetConnectionService();
            _internetService.Start();
            _internetService.OnNetworkConnectionChanged += InternetService_OnNetworkConnectionChanged;
            _logger.Log(LogLevel.Information, "Initialized services.");
        }

        private void InitializeView()
        {
            if (!_keyStore.WalletExists)
            {
                // neither mnemonic is saved nor private key file has been imported so need to setup
                _importWalletViewModel ??= new ImportWalletViewModel(_appState);
                CurrentView = _importWalletViewModel;
                _importWalletViewModel.Confirm.Subscribe(OnWalletImport);
            }
            else if (_keyStore.IsEncrypted)
            {
                // keystore is encrypted, request password
                _unlockWalletViewModel ??= new UnlockWalletViewModel();
                _unlockWalletViewModel.Confirm.Subscribe(OnWalletUnlock);
                CurrentView = _unlockWalletViewModel;
            }
            else
            {
                // keystore is not encrypted, initialize wallets
                _keyStoreService.InitializeWallets();
            }
        }

        /// <summary>
        /// Handles errors while loading the wallets. 
        /// In this case this only handles possible changes to the location of private key files in order to warn the user.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private async void _keyStoreService_OnLoadingError(object sender, Services.Events.KeyStoreLoadingErrorEventArgs e)
        {
            var msg = "One or more private key wallet(s) could not be loaded:\n";

            foreach (var w in e.UnloadedWallets)
            {
                msg += $"{w}\n";
            }

            var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
            {
                ContentHeader = "Error Loading Private Keys",
                SupportingText = msg,
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
        }

        /// <summary>
        /// Restores the different wallets and currently selected wallet from the <see cref="IWalletService"/> snapshot.
        /// </summary>
        /// <param name="onlyRefreshLists">Whether to only refresh the <see cref="SubWalletType"/> lists or also the <see cref="CurrentWallet"/>.</param>
        private void RestoreFromWalletSnapshot()
        {
            foreach (var w in _walletService.Wallets)
            {
                Wallets.Add(w);
            }
            if (_walletService.CurrentWallet != null)
            {
                CurrentWallet = _walletService.CurrentWallet;
            }
        }

        /// <summary>
        /// Handles a change to the <see cref="IWalletService"/> state, this could be an addition, removal or change of alias.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void _walletService_OnWalletServiceStateChanged(object sender, Services.Events.WalletServiceStateChangedEventArgs e)
        {
            if (KeyStoreServiceState != KeyStoreServiceState.Done) return;

            if (e.StateChange == WalletServiceStateChange.Addition)
            {
                Dispatcher.UIThread.Post(delegate { AddWallet(e.Wallet); });
            }
            else if (e.StateChange == WalletServiceStateChange.Removal)
            {
                Dispatcher.UIThread.Post(delegate { RemoveWallet(e.Wallet); });
            }
            else if (e.StateChange == WalletServiceStateChange.AliasChanged)
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
            Wallets.Remove(wallet);
        }

        /// <summary>
        /// Adds a given wallet to the collections.
        /// </summary>
        /// <param name="wallet">The wallet to add.</param>
        private void AddWallet(IWallet wallet)
        {
            Wallets.Add(wallet);
        }

        /// <summary>
        /// Edits a given wallet in the collections.
        /// </summary>
        /// <param name="wallet"></param>
        private void EditWallet(IWallet wallet)
        {
            if (wallet == null) return;
            CurrentWallet = null;
            Wallets = new ObservableCollection<IWallet>();

            RestoreFromWalletSnapshot();
        }

        /// <summary>
        /// Handles a change to the <see cref="IWalletService.CurrentWallet"/> being provided.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void _walletService_OnCurrentWalletChanged(object sender, Services.Wallets.Events.CurrentWalletChangedEventArgs e)
        {
            if (KeyStoreServiceState != KeyStoreServiceState.Done) return;
            if (e.Wallet != null && e.Wallet != CurrentWallet)
                CurrentWallet = e.Wallet;
        }

        /// <summary>
        /// Handles a change to the <see cref="KeyStoreService"/> startup state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void _keyStoreService_OnStartupStateChanged(object sender, Services.Events.KeyStoreServiceStateChangedEventArgs e)
        {
            KeyStoreServiceState = e.State;

            if (_keyStore.IsEncrypted && _unlockWalletViewModel != null)
            {
                _unlockWalletViewModel.ProgressStatus = e.Message;
            }

            if (!_keyStoreService.IsProcessing && KeyStoreServiceState == KeyStoreServiceState.Done)
            {
                _walletViewModel ??= new WalletViewModel(_appLifetime, _internetService, _walletService,
                    _rpcProvider, _keyStoreService, _addressBookService, _appState);
                CurrentView = _walletViewModel;
                WalletUnlocked = true;
                RestoreFromWalletSnapshot();
            }
        }

        /// <summary>
        /// Handles the user action to unlock the wallet.
        /// </summary>
        /// <param name="walletUnlock">The params.</param>
        private async void OnWalletUnlock(WalletUnlock walletUnlock)
        {
            _unlockWalletViewModel.IsProcessing = true;
            var success = await _keyStoreService.DecryptKeyStoreAndInitializeWallets(walletUnlock.Password);
            if (success)
            {
                _unlockWalletViewModel.ProgressStatus = "Wallet unlocked.";

            }
            else
            {
                _unlockWalletViewModel.ProgressStatus = "Wrong password.";
                _unlockWalletViewModel.IsProcessing = false;
            }
        }

        /// <summary>
        /// Handles the user action to import the wallet.
        /// </summary>
        /// <param name="walletImport">The params.</param>
        private async void OnWalletImport(WalletImport walletImport)
        {
            if (walletImport.PrivateKeyFilePath != string.Empty && walletImport.PrivateKeyFilePath != null)
            {
                // private key import
                await _keyStoreService.InitializeWalletWithPrivateKey(walletImport.PrivateKeyFilePath, walletImport.Alias, walletImport.Password);

                _walletViewModel ??= new WalletViewModel(_appLifetime, _internetService, _walletService, _rpcProvider, _keyStoreService, _addressBookService, _appState);
                CurrentView = _walletViewModel;
                WalletUnlocked = true;
            }
            else
            {
                // mnemonic import
                await _keyStoreService.InitializeWallet(walletImport.Mnemonic, walletImport.Password);

                _walletViewModel ??= new WalletViewModel(_appLifetime, _internetService, _walletService, _rpcProvider, _keyStoreService, _addressBookService, _appState);
                CurrentView = _walletViewModel;
                WalletUnlocked = true;
            }
        }

        /// <summary>
        /// Handles changes in the network connection.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void InternetService_OnNetworkConnectionChanged(object sender, NetworkConnectionChangedEventArgs e)
        {
            NetworkConnected = e.Connected;
            NetworkConnectionStatus = NetworkConnected ? "Online" : "Offline";
        }

        /// <summary>
        /// Change the current view.
        /// </summary>
        /// <param name="view">The new view.</param>
        public void ChangeView(string view)
        {
            switch (view)
            {
                case "Wallet":
                    _walletViewModel ??= new WalletViewModel(_appLifetime, _internetService, _walletService, _rpcProvider, _keyStoreService, _addressBookService, _appState);
                    CurrentView = _walletViewModel;
                    break;
                case "WatchOnly":
                    _watchOnlyViewModel ??= new WatchOnlyViewModel(_appLifetime, _internetService, _rpcProvider, _watchOnlyAccountStore);
                    CurrentView = _watchOnlyViewModel;
                    break;
                case "Crafter":
                    _crafterViewModel ??= new CrafterViewModel(_appLifetime, _internetService, _rpcProvider, _walletService, _nonceAccountMappingStore, _addressBookService);
                    CurrentView = _crafterViewModel;
                    break;
                case "MultiSigs":
                    _multiSigsViewModel ??= new MultiSignaturesViewModel(_appLifetime, _internetService, _rpcProvider, _walletService, _multisigAccountMappingStore, _addressBookService);
                    CurrentView = _multiSigsViewModel;
                    break;
                case "NonceAccounts":
                    _nonceAccountsViewModel ??= new NonceAccountsViewModel(_appLifetime, _internetService, _rpcProvider, _walletService, _nonceAccountMappingStore, _addressBookService);
                    CurrentView = _nonceAccountsViewModel;
                    break;
                case "Settings":
                    _settingsViewModel ??= new SettingsViewModel(_appState, _rpcProvider);
                    CurrentView = _settingsViewModel;
                    break;
                default:
                    break;
            }
        }

        // TODO: Add support for the app to stay in the tray
        //public WindowIcon TrayIcon { get; }

        private bool _walletUnlocked = false;
        public bool WalletUnlocked
        {
            get => _walletUnlocked;
            set => this.RaiseAndSetIfChanged(ref _walletUnlocked, value);
        }

        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        private bool _networkConnected;
        public bool NetworkConnected
        {
            get => _networkConnected;
            set => this.RaiseAndSetIfChanged(ref _networkConnected, value);
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
                    }
                }
            }
        }

        private ObservableCollection<IWallet> _wallets;
        public ObservableCollection<IWallet> Wallets
        {
            get => _wallets;
            set => this.RaiseAndSetIfChanged(ref _wallets, value);
        }

        private string _networkConnectionStatus = "Checking network..";
        public string NetworkConnectionStatus
        {
            get => _networkConnectionStatus;
            set => this.RaiseAndSetIfChanged(ref _networkConnectionStatus, value);
        }

        private KeyStoreServiceState _keyStoreServiceState;
        public KeyStoreServiceState KeyStoreServiceState
        {
            get => _keyStoreServiceState;
            set => this.RaiseAndSetIfChanged(ref _keyStoreServiceState, value);
        }
    }
}
