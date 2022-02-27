using Anvil.Services.Wallets.Enums;
using Anvil.Services.Wallets.SubWallets;
using Solnet.Wallet;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// Specifies functionality for a generic wallet.
    /// </summary>
    public interface IWallet
    {
        /// <summary>
        /// The sub wallet type.
        /// </summary>
        public SubWalletType SubWalletType { get; }

        /// <summary>
        /// The aliased wallet.
        /// </summary>
        public IAliasedWallet AliasedWallet { get; }

        /// <summary>
        /// The wallet address.
        /// </summary>
        public PublicKey Address { get; }

        /// <summary>
        /// The wallet's shortened address.
        /// </summary>
        public string ShortenedAddress { get; }

        /// <summary>
        /// The wallet's alias.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Request a signature of the given data.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <returns>The signature.</returns>
        public byte[] Sign(byte[] data);
    }
}
