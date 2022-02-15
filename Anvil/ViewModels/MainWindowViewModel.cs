
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

namespace Anvil.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ILogger _logger;
        private IKeyStore _keyStore;
        private IWalletService _walletService;
        private INonceAccountMappingStore _nonceAccountMappingStore;
        private IMultiSignatureAccountMappingStore _multisigAccountMappingStore;
        private IRpcClientProvider _rpcProvider;

        private ApplicationState _appState;
        private InternetConnectionService _internetService;
        private KeyStoreService _keyStoreService;

        private CrafterViewModel _crafterViewModel;
        private MultiSignaturesViewModel _multisigsViewModel;
        private WalletViewModel _walletViewModel;
        private ImportWalletViewModel _importWalletViewModel;
        private UnlockWalletViewModel _unlockWalletViewModel;
        private SettingsViewModel _settingsViewModel;

        public MainWindowViewModel(ApplicationState appState)
        {
            _logger = LoggerFactory.Create(x =>
            {
                x.AddDebug();
                x.AddSimpleConsole(o =>
                {
                    o.UseUtcTimestamp = true;
                    o.IncludeScopes = true;
                    o.ColorBehavior = LoggerColorBehavior.Enabled;
                    o.TimestampFormat = "HH:mm:ss ";
                })
                    .SetMinimumLevel(LogLevel.Trace);
            }).CreateLogger<App>();
            _logger.Log(LogLevel.Information, "Successfully attached logger, initializing modules.");
            _appState = appState;
            _rpcProvider = appState.RpcUrl != string.Empty ? new RpcClientProvider(appState.RpcUrl) : new RpcClientProvider(appState.Cluster);
            

            var nonceAccountMappingStoreConfig = new StoreConfig()
            {
                Directory = appState.StorePath,
                Name = NonceAccountMappingStore.FileName 
            };

            _nonceAccountMappingStore = new NonceAccountMappingStore(_logger, nonceAccountMappingStoreConfig);
            var multisigAccountMappingStoreConfig = new StoreConfig()
            {
                Directory = appState.StorePath,
                Name = MultiSignatureAccountMappingStore.FileName
            };

            _multisigAccountMappingStore = new MultiSignatureAccountMappingStore(_logger, multisigAccountMappingStoreConfig);
            var keyStoreConfig = new StoreConfig()
            {
                Directory = appState.StorePath,
                Name = KeyStore.FileName
            };

            _keyStore = new KeyStore(_logger, keyStoreConfig);
            _walletService = new WalletService(_keyStore);
            _keyStoreService = new KeyStoreService(_logger, _walletService, _keyStore);
            _keyStoreService.OnStartupStateChanged += _keyStoreService_OnStartupStateChanged;

            _internetService = new InternetConnectionService();
            _internetService.Start();
            _internetService.OnNetworkConnectionChanged += InternetService_OnNetworkConnectionChanged;

            if(!_keyStore.WalletExists)
            {
                // neither mnemonic is saved nor private key file has been imported so need to setup
                _importWalletViewModel ??= new ImportWalletViewModel(appState);
                CurrentView = _importWalletViewModel;
                _importWalletViewModel.Confirm.Subscribe(OnWalletImport);
            } else if (_keyStore.IsEncrypted)
            {
                // unlock wallet
                _unlockWalletViewModel ??= new UnlockWalletViewModel();
                _unlockWalletViewModel.Confirm.Subscribe(OnWalletUnlock);
                CurrentView = _unlockWalletViewModel;
            }
            else
            {
                // instantiate wallet vm
                _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider, _keyStoreService);
                CurrentView = _walletViewModel;
                WalletUnlocked = true;
            }
        }

        private void _keyStoreService_OnStartupStateChanged(object? sender, Services.Wallets.Events.KeyStoreServiceStateChangedEventArgs e)
        {
            if (_keyStore.IsEncrypted && _unlockWalletViewModel != null)
            {
                _unlockWalletViewModel.ProgressStatus = e.Message;
            }

            if(!_keyStoreService.IsProcessing && e.State == Services.Wallets.Enums.KeyStoreServiceState.Done)
            {
                _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider, _keyStoreService);
                CurrentView = _walletViewModel;
                WalletUnlocked = true;
            }
        }

        private async void OnWalletUnlock(WalletUnlock walletUnlock)
        {
            _unlockWalletViewModel.IsProcessing = true;
            var success = await _keyStoreService.DecryptKeyStoreAndInitializeWallets(walletUnlock.Password);
            if (success)
            {
                _unlockWalletViewModel.ProgressStatus = "Wallet unlocked.";
                await Task.Delay(1000);
                _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider, _keyStoreService);
                CurrentView = _walletViewModel;
                WalletUnlocked = true;

            } else
            {
                _unlockWalletViewModel.ProgressStatus = "Wrong password.";
                _unlockWalletViewModel.IsProcessing = false;
            }
        }

        private async void OnWalletImport(WalletImport walletImport)
        {
            if(walletImport.PrivateKeyFilePath != string.Empty && walletImport.PrivateKeyFilePath != null)
            {
                // private key import
                _keyStoreService.ImportPrivateKeyFile(walletImport.PrivateKeyFilePath);

                _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider, _keyStoreService);
                CurrentView = _walletViewModel;

                WalletUnlocked = true;
            } else
            {
                // mnemonic import
                await _keyStoreService.InitializeWallet(walletImport.Mnemonic, walletImport.Password);

                _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider, _keyStoreService);
                CurrentView = _walletViewModel;
                WalletUnlocked = true;
            }
        }

        private void InternetService_OnNetworkConnectionChanged(object? sender, NetworkConnectionChangedEventArgs e)
        {
            Console.WriteLine($"Connected: {e.Connected}");
            NetworkConnected = e.Connected;
            NetworkConnectionStatus = NetworkConnected ? "Online" : "Offline";
        }

        public void ChangeView(string view)
        {
            switch (view)
            {
                case "Wallet":
                    _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider, _keyStoreService);
                    CurrentView = _walletViewModel;
                    break;
                case "Crafter":
                    _crafterViewModel ??= new CrafterViewModel(_rpcProvider, _walletService, _nonceAccountMappingStore);
                    CurrentView = _crafterViewModel;
                    break;
                case "MultiSigs":
                    _multisigsViewModel ??= new MultiSignaturesViewModel(_rpcProvider, _walletService, _multisigAccountMappingStore);
                    CurrentView = _multisigsViewModel;
                    break;
                case "Settings":
                    _settingsViewModel ??= new SettingsViewModel(_appState, _rpcProvider);
                    CurrentView = _settingsViewModel;
                    break;
                default:
                    break;
            }
        }

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

        private string _networkConnectionStatus = "Checking network..";
        public string NetworkConnectionStatus
        {
            get => _networkConnectionStatus;
            set => this.RaiseAndSetIfChanged(ref _networkConnectionStatus, value);
        }
    }
}
