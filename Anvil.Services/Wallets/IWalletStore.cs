using Anvil.Services.Wallets.SubWallets;
using System.Collections.Generic;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// Specifies functionality for the wallet store.
    /// </summary>
    public interface IWalletStore
    {
        /// <summary>
        /// The mnemonic used to generate derivation index based wallets.
        /// </summary>
        public string Mnemonic { get; }

        /// <summary>
        /// The list of derivation index based wallets.
        /// </summary>
        public List<DerivationIndexWallet> DerivationIndexWallets { get; }

        /// <summary>
        /// The list of imported private key file based wallets.
        /// </summary>
        public List<PrivateKeyWallet> PrivateKeyWallets { get; }

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
        /// Edits the alias of the given derivation index based wallet.
        /// </summary>
        /// <param name="derivationIndexWallet">The derivation index wallet.</param>
        /// <param name="newAlias">The new alias.</param>
        void EditAlias(DerivationIndexWallet derivationIndexWallet, string newAlias);

        /// <summary>
        /// Edits the alias of the given private key file based wallet.
        /// </summary>
        /// <param name="privateKeyWallet">The private key wallet.</param>
        /// <param name="newAlias">The new alias.</param>
        void EditAlias(PrivateKeyWallet privateKeyWallet, string newAlias);

        /// <summary>
        /// Add a wallet by mnemonic. 
        /// </summary>
        /// <param name="mnemonic">The mnemonic.</param>
        void AddWallet(string mnemonic);
    }
}
