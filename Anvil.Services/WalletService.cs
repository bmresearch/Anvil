using Anvil.Services.Enums;
using Anvil.Services.Events;
using Anvil.Services.Store;
using Anvil.Services.Wallets;
using Anvil.Services.Wallets.Events;
using Anvil.Services.Wallets.SubWallets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Anvil.Services
{
    /// <summary>
    /// Implements a wallet service.
    /// </summary>
    public class WalletService : IWalletService, INotifyPropertyChanged
    {
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
            _privateKeyWallets = new();
            _derivationIndexWallets = new();
        }

        /// <inheritdoc cref="IWalletService.EditAlias(string, string)"/>
        public void EditAlias(string address, string newAlias)
        {
            var w = Wallets.FirstOrDefault(x => x.Address == address);
            if (w == null) return;

            w.Alias = newAlias;
            KeyStore.EditAlias(w.AliasedWallet, newAlias);

            OnWalletServiceStateChanged?.Invoke(this, new(WalletServiceStateChange.AliasChanged, (SolanaWalletService)w));
        }

        /// <inheritdoc cref="IWalletService.AddWallet(string)"/>
        public void AddWallet(string mnemonic)
        {
            /// Sanity check mnemonic string
            if (string.IsNullOrEmpty(mnemonic)) return;

            /// Check if the mnemonic has already been added to the wallet service
            if (_mnemonic != null) return;
            _mnemonic = mnemonic;
            PropertyChanged?.Invoke(this, new(nameof(MnemonicImported)));

            /// Check if the mnemonic has already been added to the key store
            if (!string.IsNullOrEmpty(KeyStore.Wallet.Mnemonic)) return;
            KeyStore.AddWallet(_mnemonic);
        }

        /// <inheritdoc cref="IWalletService.AddWallet(IPrivateKeyWallet)"/>
        public IWallet AddWallet(PrivateKeyWallet pkWallet)
        {
            /// Check if this private key wallet has already been added to the wallet service.
            if (pkWallet.PrivateKey != null)
            {
                if (_privateKeyWallets.Any(x => x.PrivateKey == pkWallet.PrivateKey)) return null;
            }
            else if (pkWallet.Path != null)
            {
                if (_privateKeyWallets.Any(x => x.Path == pkWallet.Path)) return null;
            }
            
            _privateKeyWallets.Add(pkWallet);

            SolanaWalletService solanaWalletService = new(pkWallet);
            Wallets.Add(solanaWalletService);

            if (CurrentWallet == null)
                CurrentWallet = solanaWalletService;

            OnWalletServiceStateChanged?.Invoke(this, new(WalletServiceStateChange.Addition, solanaWalletService));

            /// Check if this private key wallet has already been added to the key store
            if (pkWallet.PrivateKey != null)
            {
                if (KeyStore.Wallet.PrivateKeyWallets.Any(x => x.PrivateKey == pkWallet.PrivateKey)) return solanaWalletService;
            }
            else if (pkWallet.Path != null)
            {
                if (KeyStore.Wallet.PrivateKeyWallets.Any(x => x.Path == pkWallet.Path)) return solanaWalletService;
            }
            KeyStore.AddWallet(pkWallet);

            return solanaWalletService;
        }

        /// <inheritdoc cref="IWalletService.AddWallet(IDerivationIndexWallet)"/>
        public IWallet AddWallet(DerivationIndexWallet derivationWallet)
        {
            /// Check if this derivation index wallet has already been added to the wallet service.
            if (_derivationIndexWallets.Any(x => x.DerivationIndex == derivationWallet.DerivationIndex)) return null;
            _derivationIndexWallets.Add(derivationWallet);

            SolanaWalletService solanaWalletService = new(_mnemonic, derivationWallet);
            Wallets.Add(solanaWalletService);

            if (CurrentWallet == null)
                CurrentWallet = solanaWalletService;

            OnWalletServiceStateChanged?.Invoke(this, new(WalletServiceStateChange.Addition, solanaWalletService));

            /// Check if this derivation index wallet has already been added to the key store
            if (KeyStore.Wallet.DerivationIndexWallets.Any(x => x.DerivationIndex == derivationWallet.DerivationIndex)) return solanaWalletService;
            derivationWallet.Alias = $"Account {derivationWallet.DerivationIndex + 1}";
            KeyStore.AddWallet(derivationWallet);

            return solanaWalletService;
        }

        /// <inheritdoc cref="IWallet.GenerateNewWallet"/>
        public IWallet GenerateNewWallet()
        {
            int idx = KeyStore.Wallet.DerivationIndexWallets.Select(w => w.DerivationIndex).DefaultIfEmpty(0).Max() + 1;

            DerivationIndexWallet derivationWallet = new()
            {
                DerivationIndex = idx,
            };

            var wallet = AddWallet(derivationWallet);

            return wallet;
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
        /// Whether a mnemonic has been imported.
        /// </summary>
        public bool MnemonicImported { get => _mnemonic != null; }

        /// <summary>
        /// The list of wallets.
        /// </summary>
        public List<IWallet> Wallets { get; private set; }

        /// <summary>
        /// The mnemonic.
        /// </summary>
        private string _mnemonic;

        /// <summary>
        /// The list of private key file based wallets.
        /// </summary>
        private List<IPrivateKeyWallet> _privateKeyWallets;

        /// <summary>
        /// The list of derivation index based wallets.
        /// </summary>
        private List<IDerivationIndexWallet> _derivationIndexWallets;

        /// <summary>
        /// The event raised whenever the current wallet changes.
        /// </summary>
        public event EventHandler<CurrentWalletChangedEventArgs> OnCurrentWalletChanged;

        /// <summary>
        /// The event raised whenever the current wallet changes.
        /// </summary>
        public event EventHandler<WalletServiceStateChangedEventArgs> OnWalletServiceStateChanged;

        /// <summary>
        /// An event raised whenever certain properties change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
