
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

namespace Anvil.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ApplicationState _appState;
        private InternetConnectionService _internetService;
        private KeyStoreService _keyStoreService;
        private IWalletService _walletService;
        private INonceAccountMappingStore _nonceAccountMappingStore;
        private IMultiSignatureAccountMappingStore _multisigAccountMappingStore;
        private IRpcClientProvider _rpcProvider;

        private CrafterViewModel _crafterViewModel;
        private MultiSignaturesViewModel _multisigsViewModel;
        private WalletViewModel _walletViewModel;
        private ImportWalletViewModel _importWalletViewModel;
        private UnlockWalletViewModel _unlockWalletViewModel;
        private SettingsViewModel _settingsViewModel;

        public MainWindowViewModel(ApplicationState appState)
        {
            _appState = appState;
            _rpcProvider = appState.RpcUrl != string.Empty ? new RpcClientProvider(appState.RpcUrl) : new RpcClientProvider(appState.Cluster);
            _walletService = new WalletService();
            _nonceAccountMappingStore = new NonceAccountMappingStore();
            _multisigAccountMappingStore = new MultiSignatureAccountMappingStore();

            _keyStoreService = new KeyStoreService(_walletService);

            _internetService = new InternetConnectionService();
            _internetService.Start();
            _internetService.OnNetworkConnectionChanged += InternetService_OnNetworkConnectionChanged;

            if(!_appState.MnemonicSaved && _appState.PrivateKeyFilePath == string.Empty)
            {
                // neither mnemonic is saved nor private key file path has been set so need to import
                _importWalletViewModel ??= new ImportWalletViewModel(appState);
                CurrentView = _importWalletViewModel;
                _importWalletViewModel.Confirm.Subscribe(OnWalletImport);
            } else
            {
                if(_appState.PrivateKeyFilePath != string.Empty)
                {
                    // import private key file
                    _keyStoreService.ImportPrivateKeyFile(_appState.PrivateKeyFilePath);

                    _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider);
                    CurrentView = _walletViewModel;

                    WalletUnlocked = true;

                } else if (appState.IsEncrypted)
                {
                    // unlock mnemonic keystore
                    _unlockWalletViewModel ??= new UnlockWalletViewModel();
                    CurrentView = _unlockWalletViewModel;
                } else
                {
                    // TODO: instantiate wallet and stuff
                    _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider);
                    CurrentView = _walletViewModel;
                }
            }
        }

        private void OnWalletImport(WalletImport walletImport)
        {
            if(walletImport.PrivateKeyFilePath != string.Empty && walletImport.PrivateKeyFilePath != null)
            {
                // private key import
                _appState.PrivateKeyFilePath = walletImport.PrivateKeyFilePath;
                _keyStoreService.ImportPrivateKeyFile(_appState.PrivateKeyFilePath);

                _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider);
                CurrentView = _walletViewModel;

                WalletUnlocked = true;
            } else
            {
                // mnemonic import
                _appState.MnemonicStoreFilePath = walletImport.MnemonicStorePath;
                _appState.MnemonicSaved = walletImport.SaveMnemonic;

                _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider);
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
                    _walletViewModel ??= new WalletViewModel(_walletService, _rpcProvider);
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
