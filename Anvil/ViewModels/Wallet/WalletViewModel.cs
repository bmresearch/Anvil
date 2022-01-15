using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.Services.Wallets;
using ReactiveUI;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Wallet;
using System.Collections.ObjectModel;

namespace Anvil.ViewModels.Wallet
{
    public class WalletViewModel : ViewModelBase
    {
        private IWalletService _walletService;
        private IRpcClientProvider _rpcClientProvider;
        private IRpcClient _rpcClient => _rpcClientProvider.Client;

        public WalletViewModel(IWalletService walletService, IRpcClientProvider rpcClientProvider)
        {
            _rpcClientProvider = rpcClientProvider;
            _rpcClientProvider.OnClientChanged += _rpcClientProvider_OnClientChanged;
            _walletService = walletService;
            _walletService.OnCurrentWalletChanged += _walletService_OnCurrentWalletChanged;

            WalletsCollection = new();

            CurrentWallet = walletService.CurrentWallet;

            GetAccountBalance();
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

        private async void GetAccountBalance()
        {
            var balance = await _rpcClient.GetBalanceAsync(CurrentWallet.Wallet.Account.PublicKey);

            if (balance.WasRequestSuccessfullyHandled)
                CurrentBalance = (double) balance.Result.Value / SolHelper.LAMPORTS_PER_SOL;
        }


        public PublicKey PublicKey { get => CurrentWallet.Wallet.Account.PublicKey; }

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
            set => this.RaiseAndSetIfChanged(ref _currentWallet, value);
        }

        private ObservableCollection<IWallet> _walletsCollection;
        public ObservableCollection<IWallet> WalletsCollection
        {
            get => _walletsCollection;
            set => this.RaiseAndSetIfChanged(ref _walletsCollection, value);
        }
    }
}
