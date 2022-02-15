using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// The wallet address.
        /// </summary>
        public PublicKey Address { get; }

        /// <summary>
        /// The wallet's shortened address.
        /// </summary>
        public string ShortenedAddress { get; }

        /// <summary>
        /// Request a signature of the given data.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <returns>The signature.</returns>
        public byte[] Sign(byte[] data);
    }
}
