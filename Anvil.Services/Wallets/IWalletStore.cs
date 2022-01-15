using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// The wallet store for <see cref="IDerivationPathWallet"/> and <see cref="IPrivateKeyWallet"/>s.
    /// </summary>
    public interface IWalletStore
    {
        /// <summary>
        /// The mnemonic.
        /// </summary>
        public string Mnemonic { get; }

        /// <summary>
        /// The list of derivation path based wallets.
        /// </summary>
        public List<IDerivationPathWallet> DerivationPathWallets { get; }

        /// <summary>
        /// The list of private key file based wallets.
        /// </summary>
        public List<IPrivateKeyWallet> PrivateKeyWallets { get; }
    }
}
