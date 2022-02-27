using Anvil.Services.Wallets.SubWallets;
using Solnet.Wallet;
using System.Collections.Generic;

namespace Anvil.Services.Store.Models
{
    /// <summary>
    /// A mapping between a <see cref="MultiSignatureAccount"/>'s <see cref="PublicKey"/> and it's signer's.
    /// </summary>
    public class MultiSignatureAccountMapping : IAliasedWallet
    {
        /// <summary>
        /// The alias of the multi signature account. Used for search features.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The <see cref="PublicKey"/> of the multi signature account.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The <see cref="PublicKey"/>s of the multi signature account signers.
        /// </summary>
        public List<string> Signers { get; set; }

        /// <summary>
        /// The minimum number of signers for the account.
        /// </summary>
        public int MinimumSigners { get; set; }

        /// <summary>
        /// The shortened address
        /// </summary>
        public string ShortenedAddress { get => Address[..6] + "..." + Address[^6..]; }
    }
}
