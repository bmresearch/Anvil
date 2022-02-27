using Anvil.Services.Store.Events;
using Anvil.Services.Store.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;

namespace Anvil.Services
{
    /// <summary>
    /// Specifies functionality for a <see cref="NonceAccountMapping"/> store.
    /// </summary>
    public interface INonceAccountMappingStore
    {
        /// <summary>
        /// Adds a <see cref="NonceAccountMapping"/> to the store.
        /// </summary>
        /// <param name="mapping">The mapping to add.</param>
        void AddMapping(NonceAccountMapping mapping);

        /// <summary>
        /// Gets the first found <see cref="NonceAccountMapping"/> by it's authority.
        /// </summary>
        /// <param name="authority">The <see cref="PublicKey"/> of the <see cref="NonceAccount"/> authority.</param>
        /// <returns>The <see cref="NonceAccountMapping"/>.</returns>
        NonceAccountMapping GetMapping(PublicKey authority);

        /// <summary>
        /// The existing <see cref="NonceAccountMapping"/>s.
        /// </summary>
        List<NonceAccountMapping> NonceAccountMappings { get; }

        /// <summary>
        /// An event raised whenever the store state changes.
        /// </summary>
        event EventHandler<NonceAccountMappingStateChangedEventArgs> OnStateChanged;
    }
}
