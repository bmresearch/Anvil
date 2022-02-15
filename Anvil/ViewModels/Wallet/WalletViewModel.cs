using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.Services.Wallets;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ReactiveUI;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Wallet;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Wallet
{
    public class WalletViewModel : ViewModelBase
    {
        private IWalletService _walletService;
        private IRpcClientProvider _rpcClientProvider;
        private KeyStoreService _keyStoreService;
        private IRpcClient _rpcClient => _rpcClientProvider.Client;

        public WalletViewModel(IWalletService walletService, IRpcClientProvider rpcClientProvider, KeyStoreService keyStoreService)
        {
            _rpcClientProvider = rpcClientProvider;
            _rpcClientProvider.OnClientChanged += _rpcClientProvider_OnClientChanged;
            _walletService = walletService;
            _walletService.OnCurrentWalletChanged += _walletService_OnCurrentWalletChanged;
            _walletService.OnWalletServiceStateChanged += _walletService_OnWalletServiceStateChanged;
            _keyStoreService = keyStoreService;

            DerivationWalletsColleciton = new();
            PrivateKeyWalletsCollection = new();

            if (_walletService.CurrentWallet != null)
            {
                CurrentWallet = _walletService.CurrentWallet; 
                GetAccountBalance();
            }

            HandleWalletSnapshot();
        }

        public void CopyAddressToClipboard()
        {
            Application.Current.Clipboard.SetTextAsync(_walletService.CurrentWallet.Address.Key);
        }

        public async void ImportPrivateKey()
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
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var selected = await ofd.ShowAsync(desktop.MainWindow);
                if (selected == null) return;
                if (selected.Length > 0)
                {
                    if (!File.Exists(selected[0])) return;
                    _keyStoreService.ImportPrivateKeyFile(selected[0]);
                }
            }
        }

        private void HandleWalletSnapshot()
        {
            foreach (var w in _walletService.Wallets)
            {
                switch (w.SubWalletType)
                {
                    case SubWalletType.DerivationIndex:
                        DerivationWalletsColleciton.Add(w);
                        break;
                    case SubWalletType.PrivateKey:
                        PrivateKeyWalletsCollection.Add(w);
                        break;
                }
            }
        }

        private void _rpcClientProvider_OnClientChanged(object? sender, Services.Rpc.Events.RpcClientChangedEventArgs e)
        {
            GetAccountBalance();
        }

        private void _walletService_OnCurrentWalletChanged(object? sender, Services.Wallets.Events.CurrentWalletChangedEventArgs e)
        {
            CurrentWallet = e.Wallet;
            GetAccountBalance();
        }

        private void _walletService_OnWalletServiceStateChanged(object? sender, Services.Wallets.Events.WalletServiceStateChangedEventArgs e)
        {
            if (e.StateChange == Services.Wallets.Enums.WalletServiceStateChange.Addition)
            {
                Dispatcher.UIThread.Post(delegate { AddWallet(e.Wallet); });
            }
            else if (e.StateChange == Services.Wallets.Enums.WalletServiceStateChange.Removal)
            {
                Dispatcher.UIThread.Post(delegate { RemoveWallet(e.Wallet); });
            }
        }

        private void RemoveWallet(IWallet wallet)
        {
            switch (wallet.SubWalletType)
            {
                case SubWalletType.DerivationIndex:
                    DerivationWalletsColleciton.Remove(wallet);
                    break;
                case SubWalletType.PrivateKey:
                    PrivateKeyWalletsCollection.Remove(wallet);
                    break;
            }
        }

        private void AddWallet(IWallet wallet)
        {
            switch (wallet.SubWalletType)
            {
                case SubWalletType.DerivationIndex:
                    DerivationWalletsColleciton.Add(wallet);
                    break;
                case SubWalletType.PrivateKey:
                    PrivateKeyWalletsCollection.Add(wallet);
                    break;
            }
        }

        private async void GetAccountBalance()
        {
            var balance = await _rpcClient.GetBalanceAsync(CurrentWallet.Address);

            if (balance.WasRequestSuccessfullyHandled)
                CurrentBalance = (double)balance.Result.Value / SolHelper.LAMPORTS_PER_SOL;
        }

        private double _currentBalance;
        public double CurrentBalance
        {
            get => _currentBalance;
            set => this.RaiseAndSetIfChanged(ref _currentBalance, value);
        }

        private IWallet _currentWallet;
        public IWallet CurrentWallet
        {
            get => _currentWallet;
            set
            {
                if (value != _walletService.CurrentWallet && value != null)
                {
                    Task.Run(delegate { _walletService.ChangeWallet(value); });
                    this.RaiseAndSetIfChanged(ref _currentWallet, value);
                }else if(_currentWallet == null)
                {
                    this.RaiseAndSetIfChanged(ref _currentWallet, value);
                }
            }
        }

        private ObservableCollection<IWallet> _drvwCollection;
        public ObservableCollection<IWallet> DerivationWalletsColleciton
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
    }
}
