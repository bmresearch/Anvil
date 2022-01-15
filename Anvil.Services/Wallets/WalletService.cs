using Anvil.Services.Wallets.Events;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets
{
    public class WalletService : IWalletService
    {
        private List<IPrivateKeyWallet> privateKeyWallets;
        private List<IDerivationPathWallet> derivationPathWallets;

        public WalletService()
        {
        }

        public void AddWallet(IPrivateKeyWallet pkWallet)
        {
            privateKeyWallets ??= new();

            if (!privateKeyWallets.Any(x => x.Address == pkWallet.Address))
                privateKeyWallets.Add(pkWallet);

            if (CurrentWallet == null)
                CurrentWallet = pkWallet;
        }

        public void AddWallet(IDerivationPathWallet derivationWallet)
        {
            derivationPathWallets ??= new();

            if (!derivationPathWallets.Any(x => x.Address == derivationWallet.Address))
                derivationPathWallets.Add(derivationWallet);

            if (CurrentWallet == null)
                CurrentWallet = derivationWallet;
        }

        public void ChangeWallet(IWallet wallet)
        {
            throw new NotImplementedException();
        }

        private IWallet _currentWallet;
        public IWallet CurrentWallet 
        {
            get => _currentWallet;
            set
            {
                _currentWallet = value;
                OnCurrentWalletChanged?.Invoke(this, new CurrentWalletChangedEventArgs(value));
            }
        }

        public List<IWallet> Wallets => throw new NotImplementedException();

        /// <summary>
        /// The event raised whenever the current wallet changes.
        /// </summary>
        public event EventHandler<CurrentWalletChangedEventArgs> OnCurrentWalletChanged;
    }
}
