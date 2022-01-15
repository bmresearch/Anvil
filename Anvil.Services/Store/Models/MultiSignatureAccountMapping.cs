using Solnet.Wallet;
using System.Collections.Generic;

namespace Anvil.Services.Store.Models
{
    /// <summary>
    /// A mapping between a <see cref="MultiSignatureAccount"/>'s <see cref="PublicKey"/> and it's signer's.
    /// </summary>
    public class MultiSignatureAccountMapping
    {
        /// <summary>
        /// The <see cref="PublicKey"/> of the multi signature account.
        /// </summary>
        public PublicKey MultiSignature { get; }

        /// <summary>
        /// The <see cref="PublicKey"/>s of the multi signature account signers.
        /// </summary>
        public List<PublicKey> Signers { get; }

        /// <summary>
        /// The minimum number of signers for the account.
        /// </summary>
        public int MinimumSigners { get; }
    }
}
