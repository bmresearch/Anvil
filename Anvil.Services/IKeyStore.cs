using Anvil.Services.Store.Events;
using Anvil.Services.Store.State;
using Anvil.Services.Wallets;
using Anvil.Services.Wallets.SubWallets;
using System;
using System.Collections.Generic;

namespace Anvil.Services.Store
{
    /// <summary>
    /// Specifies functionality for the keystore.
    /// </summary>
    public interface IKeyStore
    {
        /// <summary>
        /// Whether the wallet is encrypted.
        /// </summary>
        bool IsEncrypted { get; set; }

        /// <summary>
        /// The wallet store.
        /// </summary>
        WalletStore Wallet { get; set; }

        /// <summary>
        /// Whether the wallet exists.
        /// </summary>
        bool WalletExists { get; set; }

        /// <summary>
        /// The wallet json, if it exists, the wallet is encrypted.
        /// </summary>
        string WalletJson { get; set; }

        /// <summary>
        /// Triggers a key store persistance update.
        /// </summary>
        /// <param name="state">The key store state to persist.</param>
        void Persist(KeyStoreState state);

        /// <summary>
        /// Edit the alias of an aliased wallet.
        /// </summary>
        /// <param name="aliasedWallet">The aliased wallet.</param>
        /// <param name="newAlias">The new alias.</param>
        void EditAlias(IAliasedWallet aliasedWallet, string newAlias);

        /// <summary>
        /// Add a wallet by derivation index.
        /// </summary>
        /// <param name="derivationIndexWallet">The derivation index wallet.</param>
        void AddWallet(DerivationIndexWallet derivationIndexWallet);

        /// <summary>
        /// Add a wallet by private key.
        /// </summary>
        /// <param name="privateKeyWallet">The private key wallet.</param>
        void AddWallet(PrivateKeyWallet privateKeyWallet);

        /// <summary>
        /// Remove a wallet by private key.
        /// </summary>
        /// <param name="privateKeyWallets">The private key wallets.</param>
        void RemoveWallets(List<PrivateKeyWallet> privateKeyWallets);

        /// <summary>
        /// Remove a private key based wallet.
        /// </summary>
        /// <param name="privateKeyWallet">The private key wallet.</param>
        void RemoveWallet(PrivateKeyWallet privateKeyWallet);

        /// <summary>
        /// Remove a derivation index based wallet.
        /// </summary>
        /// <param name="derivationWallet">The derivation index wallet.</param>
        void RemoveWallet(DerivationIndexWallet derivationWallet);

        /// <summary>
        /// Add a wallet by mnemonic. 
        /// </summary>
        /// <param name="mnemonic">The mnemonic.</param>
        void AddWallet(string mnemonic);

        /// <summary>
        /// An event thrown whenever the keystore has had an update.
        /// </summary>
        event EventHandler<KeyStoreUpdateEventArgs> OnUpdate;
    }
}