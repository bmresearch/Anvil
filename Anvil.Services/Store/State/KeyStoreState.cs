using Anvil.Services.Store.Events;
using Anvil.Services.Wallets;
using Anvil.Services.Wallets.SubWallets;
using System;
using System.Collections.Generic;

namespace Anvil.Services.Store.State
{
    /// <summary>
    /// Represents the keystore state.
    /// </summary>
    public class KeyStoreState
    {
        /// <summary>
        /// Whether a wallet exists.
        /// </summary>
        private bool _walletExists;

        /// <summary>
        /// Whether a wallet exists.
        /// </summary>
        public bool WalletExists 
        { 
            get => _walletExists;
            set 
            { 
                _walletExists = value;
                OnStateChanged?.Invoke(this, new(this));
            }
        }

        /// <summary>
        /// Whether a wallet exists.
        /// </summary>
        private bool _isEncrypted;

        /// <summary>
        /// Whether the wallet is encrypted.
        /// </summary>
        public bool IsEncrypted
        {
            get => _isEncrypted;
            set
            {
                _isEncrypted = value;
                OnStateChanged?.Invoke(this, new(this));
            }
        }

        /// <summary>
        /// The actual main wallet.
        /// <remarks>
        /// In cases where the user does not provide a password to encrypt the main wallet it is stored as plaintext.
        /// Otherwise to restore the wallets the <c>KeyStoreJson</c> should be decrypted to retrieve the keystore bytes to deserialize into the actual structure.
        /// </remarks>
        /// </summary>
        public WalletStore Wallet { get; set; }

        /// <summary>
        /// The encrypted main wallet JSON.
        /// <remarks>
        /// In cases where the user does not provide a password to encrypt the main wallet it is stored as plaintext.
        /// Otherwise to restore the wallets the <c>KeyStoreJson</c> should be decrypted to retrieve the keystore bytes to deserialize into the actual structure.
        /// </remarks>
        /// </summary>
        private string _walletJson;

        /// <summary>
        /// The encrypted main wallet JSON.
        /// <remarks>
        /// In cases where the user does not provide a password to encrypt the main wallet it is stored as plaintext.
        /// Otherwise to restore the wallets the <c>KeyStoreJson</c> should be decrypted to retrieve the keystore bytes to deserialize into the actual structure.
        /// </remarks>
        /// </summary>
        public string WalletJson
        {
            get => _walletJson;
            set
            {
                _walletJson = value;
                OnStateChanged?.Invoke(this, new(this));
            }
        }

        /// <inheritdoc cref="IWalletStore.EditAlias(DerivationIndexWallet, string)"/>
        public void EditAlias(DerivationIndexWallet derivationIndexWallet, string newAlias)
        {
            Wallet.EditAlias(derivationIndexWallet, newAlias);
            OnStateChanged?.Invoke(this, new(this));
        }

        /// <inheritdoc cref="IWalletStore.EditAlias(PrivateKeyWallet, string)"/>
        public void EditAlias(PrivateKeyWallet privateKeyWallet, string newAlias)
        {
            Wallet.EditAlias(privateKeyWallet, newAlias);
            OnStateChanged?.Invoke(this, new(this));
        }

        /// <inheritdoc cref="IWalletStore.AddWallet(DerivationIndexWallet)"/>
        public void AddWallet(DerivationIndexWallet derivationIndexWallet)
        {
            Wallet.AddWallet(derivationIndexWallet);
            OnStateChanged?.Invoke(this, new(this));
        }

        /// <inheritdoc cref="IWalletStore.AddWallet(string)"/>
        public void AddWallet(string mnemonic)
        {
            Wallet.AddWallet(mnemonic);
            OnStateChanged?.Invoke(this, new(this));
        }

        /// <inheritdoc cref="IWalletStore.AddWallet(PrivateKeyWallet)"/>
        public void AddWallet(PrivateKeyWallet privateKeyWallet)
        {
            Wallet.AddWallet(privateKeyWallet);
            OnStateChanged?.Invoke(this, new(this));
        }

        /// <inheritdoc cref="IWalletStore.RemoveWallet(List{PrivateKeyWallet})"/>
        public void RemoveWallets(List<PrivateKeyWallet> privateKeyWallets)
        {
            Wallet.RemoveWallets(privateKeyWallets);
            OnStateChanged?.Invoke(this, new(this));
        }

        /// <summary>
        /// An event raised whenever the state changes.
        /// </summary>
        public event EventHandler<KeyStoreStateChangedEventArgs> OnStateChanged;
    }
}
