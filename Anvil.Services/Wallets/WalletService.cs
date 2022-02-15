using Anvil.Services.Store;
using Anvil.Services.Wallets.Events;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// Implements a wallet service.
    /// </summary>
    public class WalletService : IWalletService
    {
        /// <summary>
        /// The wallet store.
        /// </summary>
        private IWalletStore WalletStore { get; set; }

        /// <summary>
        /// The key store.
        /// </summary>
        private IKeyStore KeyStore { get; set; }

        /// <summary>
        /// Initialize the wallet service.
        /// </summary>
        public WalletService(IKeyStore keyStore)
        {
            KeyStore = keyStore;
            Wallets = new();
            privateKeyWallets = new();
            derivationIndexWallets = new();
        }

        /// <inheritdoc cref="IWalletService.AddWallet(IWalletStore)"/>
        public IWallet AddWallet(IWalletStore wallet)
        {
            // only allow one mnemonic based wallet
            if (WalletStore != null) return null;
            WalletStore = wallet;

            SolanaWalletService solanaWalletService = new SolanaWalletService(wallet);
            Wallets.Add(solanaWalletService);

            if (CurrentWallet == null)
                CurrentWallet = solanaWalletService;

            OnWalletServiceStateChanged?.Invoke(this, new(Enums.WalletServiceStateChange.Addition, solanaWalletService));

            return solanaWalletService;
        }

        /// <inheritdoc cref="IWalletService.AddWallet(IPrivateKeyWallet)"/>
        public IWallet AddWallet(PrivateKeyWallet pkWallet)
        {
            if (!privateKeyWallets.Any(x => x.Path == pkWallet.Path))
            {
                privateKeyWallets.Add(pkWallet);

                SolanaWalletService solanaWalletService = new(pkWallet);
                Wallets.Add(solanaWalletService);

                if (CurrentWallet == null)
                    CurrentWallet = solanaWalletService;

                OnWalletServiceStateChanged?.Invoke(this, new(Enums.WalletServiceStateChange.Addition, solanaWalletService));

                if (!KeyStore.Wallet.PrivateKeyWallets.Any(x => x.Path == pkWallet.Path))
                {
                    KeyStore.AddWallet(pkWallet);
                }
                return solanaWalletService;
            }

            return null;
        }

        /// <inheritdoc cref="IWalletService.AddWallet(IDerivationIndexWallet)"/>
        public IWallet AddWallet(DerivationIndexWallet derivationWallet)
        {
            if (!derivationIndexWallets.Any(x => x.DerivationIndex == derivationWallet.DerivationIndex))
            {
                derivationIndexWallets.Add(derivationWallet);

                SolanaWalletService solanaWalletService = new(WalletStore, derivationWallet);
                Wallets.Add(solanaWalletService);

                if (CurrentWallet == null)
                    CurrentWallet = solanaWalletService;

                OnWalletServiceStateChanged?.Invoke(this, new(Enums.WalletServiceStateChange.Addition, solanaWalletService));

                if (!KeyStore.Wallet.DerivationIndexWallets.Any(x => x.DerivationIndex == derivationWallet.DerivationIndex))
                {
                    KeyStore.AddWallet(derivationWallet);
                }
                return solanaWalletService;
            }


            return null;
        }

        /// <inheritdoc cref="IWalletService.ChangeWallet(IWallet)"/>
        public void ChangeWallet(IWallet wallet)
        {
            CurrentWallet = wallet;
        }

        /// <summary>
        /// The current wallet.
        /// </summary>
        private IWallet _currentWallet;

        /// <summary>
        /// The current wallet.
        /// </summary>
        public IWallet CurrentWallet
        {
            get => _currentWallet;
            private set
            {
                _currentWallet = value;
                OnCurrentWalletChanged?.Invoke(this, new CurrentWalletChangedEventArgs(value));
            }
        }

        /// <summary>
        /// The list of wallets.
        /// </summary>
        public List<IWallet> Wallets { get; private set; }

        /// <summary>
        /// The list of private key file based wallets.
        /// </summary>
        private List<IPrivateKeyWallet> privateKeyWallets;

        /// <summary>
        /// The list of derivation index based wallets.
        /// </summary>
        private List<IDerivationIndexWallet> derivationIndexWallets;

        /// <summary>
        /// The event raised whenever the current wallet changes.
        /// </summary>
        public event EventHandler<CurrentWalletChangedEventArgs> OnCurrentWalletChanged;

        /// <summary>
        /// The event raised whenever the current wallet changes.
        /// </summary>
        public event EventHandler<WalletServiceStateChangedEventArgs> OnWalletServiceStateChanged;
    }
}
