using Anvil.Services.Store.Events;
using Anvil.Services.Store.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anvil.Services.Store.State
{
    /// <summary>
    /// The <see cref="NonceAccountMappingStore"/> state.
    /// </summary>
    public class NonceAccountMappingState
    {
        /// <summary>
        /// Add a new mapping.
        /// </summary>
        /// <param name="mapping">The mapping to add.</param>
        public void AddMapping(NonceAccountMapping mapping)
        {
            NonceAccountMappings.Add(mapping);
            OnStateChanged?.Invoke(this, new NonceAccountMappingStateChangedEventArgs(this));
        }

        /// <summary>
        /// Get a mapping for the given authority.
        /// </summary>
        /// <param name="authority">The authority.</param>
        /// <returns>The mapping.</returns>
        public NonceAccountMapping GetMapping(PublicKey authority)
        {
            return NonceAccountMappings.FirstOrDefault(x => x.Authority == authority.Key);
        }

        /// <summary>
        /// The mappings.
        /// </summary>
        public List<NonceAccountMapping> NonceAccountMappings
        {
            get; set;
        }

        /// <summary>
        /// An event raised whenever a new mapping is added.
        /// </summary>
        public event EventHandler<NonceAccountMappingStateChangedEventArgs> OnStateChanged;
    }
}
