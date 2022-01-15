using Anvil.Services.Store.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services
{
    /// <summary>
    /// Specifies functionality for a <see cref="MultiSignatureAccountMappingStore"/> store.
    /// </summary>
    public interface IMultiSignatureAccountMappingStore
    {
        /// <summary>
        /// Adds a new <see cref="MultiSignatureAccountMapping"/> to the store.
        /// </summary>
        /// <param name="mapping">The mapping to add.</param>
        void AddMapping(MultiSignatureAccountMapping mapping);

        /// <summary>
        /// Gets the <see cref="MultiSignatureAccountMapping"/> for the given <see cref="MultiSignatureAccount"/> <see cref="PublicKey"/>.
        /// </summary>
        /// <param name="account">The multi sig account's public key.</param>
        /// <returns>The <see cref="MultiSignatureAccountMapping"/>.</returns>
        MultiSignatureAccountMapping GetMapping(PublicKey account);

        /// <summary>
        /// The existing <see cref="MultiSignatureAccountMapping"/>s.
        /// </summary>
        List<MultiSignatureAccountMapping> MultiSignatureAccountMappings { get; }
    }
}
