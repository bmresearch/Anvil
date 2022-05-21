using Anvil.Services.Store.Config;
using Anvil.Services.Store.Events;
using Anvil.Services.Store.State;
using Anvil.Services.Wallets;
using Anvil.Services.Wallets.SubWallets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Anvil.Services.Store
{
    /// <summary>
    /// Implements the keystore.
    /// </summary>
    public class KeyStore : Abstract.Store, IKeyStore
    {
        /// <summary>
        /// The state.
        /// </summary>
        private KeyStoreState _state;

        /// <summary>
        /// Initialize the keystore with the given logger and config.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The config.</param>
        public KeyStore(ILogger logger, StoreConfig config) : base(logger, config)
        {
            _state = _persistenceDriver.LoadState<KeyStoreState>();
            if (_state.Wallet == null)
            {
                _state.Wallet = new WalletStore();
                _persistenceDriver.SaveState(_state);
            }
            _state.OnStateChanged += _state_OnStateChanged;
        }

        /// <summary>
        /// Handle changes to the state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void _state_OnStateChanged(object sender, KeyStoreStateChangedEventArgs e)
        {
            if (_state.IsEncrypted)
            {
                OnUpdate?.Invoke(this, new());
            }
            else
            {
                _persistenceDriver.SaveState(_state);
            }
        }

        /// <inheritdoc cref="IKeyStore.AddWallet(DerivationIndexWallet)"/>
        public void AddWallet(DerivationIndexWallet derivationIndexWallet)
        {
            _state.AddWallet(derivationIndexWallet);
        }

        /// <inheritdoc cref="IKeyStore.AddWallet(PrivateKeyWallet)"/>
        public void AddWallet(PrivateKeyWallet privateKeyWallet)
        {
            _state.AddWallet(privateKeyWallet);
        }

        /// <inheritdoc cref="IKeyStore.RemoveWallets(PrivateKeyWallet)"/>
        public void RemoveWallets(List<PrivateKeyWallet> privateKeyWallets)
        {
            _state.RemoveWallets(privateKeyWallets);
        }

        /// <inheritdoc cref="IKeyStore.AddWallet(string)"/>
        public void AddWallet(string mnemonic)
        {
            _state.AddWallet(mnemonic);
        }

        /// <inheritdoc cref="IKeyStore.Persist"/>
        public void Persist(KeyStoreState state)
        {
            _persistenceDriver.SaveState(state);
        }

        /// <inheritdoc cref="IKeyStore.EditAlias(IAliasedWallet, string)"/>
        public void EditAlias(IAliasedWallet aliasedWallet, string newAlias)
        {
            if (aliasedWallet is DerivationIndexWallet derivationIndexWallet)
            {
                _state.EditAlias(derivationIndexWallet, newAlias);
            }
            else if (aliasedWallet is PrivateKeyWallet privateKeyWallet)
            {
                _state.EditAlias(privateKeyWallet, newAlias);
            }
        }

        /// <inheritdoc cref="IKeyStore.RemoveWallet(PrivateKeyWallet)"/>
        public void RemoveWallet(PrivateKeyWallet privateKeyWallet)
        {
            _state.RemoveWallet(privateKeyWallet);
        }

        /// <inheritdoc cref="IKeyStore.RemoveWallet(DerivationIndexWallet)"/>
        public void RemoveWallet(DerivationIndexWallet derivationWallet)
        {
            _state.RemoveWallet(derivationWallet);
        }

        /// <summary>
        /// The wallet store.
        /// </summary>
        public WalletStore Wallet
        {
            get => _state.Wallet;
            set => _state.Wallet = value;
        }

        /// <summary>
        /// The wallet json.
        /// </summary>
        public string WalletJson
        {
            get => _state.WalletJson;
            set => _state.WalletJson = value;
        }

        /// <summary>
        /// Whether a wallet exists.
        /// </summary>
        public bool WalletExists
        {
            get => _state.WalletExists;
            set => _state.WalletExists = value;
        }

        /// <summary>
        /// Whether the keystore is encrypted.
        /// </summary>
        public bool IsEncrypted
        {
            get => _state.IsEncrypted;
            set => _state.IsEncrypted = value;
        }

        /// <summary>
        /// The store file name.
        /// </summary>
        public const string FileName = "key.store";

        /// <inheritdoc cref="IKeyStore.OnUpdate"/>
        public event EventHandler<KeyStoreUpdateEventArgs> OnUpdate;
    }
}
