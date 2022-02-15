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
        /// Add a wallet by mnemonic. 
        /// </summary>
        /// <param name="mnemonic">The mnemonic.</param>
        void AddWallet(string mnemonic);
    }
}
