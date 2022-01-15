using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// A private key based wallet.
    /// </summary>
    public interface IPrivateKeyWallet : IWallet
    {
        /// <summary>
        /// The path to the keystore.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The address of the wallet.
        /// </summary>
        PublicKey Address { get; }

        /// <summary>
        /// Signs the data with the private key.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <returns>The signature.</returns>
        byte[] Sign(byte[] data);
    }
}
