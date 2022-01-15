using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// A derivation path based wallet.
    /// </summary>
    public interface IDerivationPathWallet : IWallet
    {
        /// <summary>
        /// The derivation index.
        /// </summary>
        int DerivationIndex { get; }

        /// <summary>
        /// The address.
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
