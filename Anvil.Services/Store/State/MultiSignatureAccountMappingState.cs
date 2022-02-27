using Anvil.Services.Store.Events;
using Anvil.Services.Store.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anvil.Services.Store.State
{
    /// <summary>
    /// The <see cref="MultiSignatureAccountMappingState"/> state.
    /// </summary>
    public class MultiSignatureAccountMappingState
    {
        /// <summary>
        /// Add a new mapping.
        /// </summary>
        /// <param name="mapping">The mapping to add.</param>
        public void AddMapping(MultiSignatureAccountMapping mapping)
        {
            MultiSignatureAccountMappings.Add(mapping);
            OnStateChanged?.Invoke(this, new MultiSignatureAccountMappingStateChangedEventArgs(this));
        }

        /// <summary>
        /// Get a mapping for the given multi signature account.
        /// </summary>
        /// <param name="account">The multi signature account.</param>
        /// <returns>The mapping.</returns>
        public MultiSignatureAccountMapping GetMapping(PublicKey account)
        {
            return MultiSignatureAccountMappings.FirstOrDefault(x => x.Address == account.Key);
        }

        /// <summary>
        /// Edits the alias of a given multi signature account.
        /// </summary>
        /// <param name="account">The multi signature account.</param>
        /// <param name="newAlias">The new alias.</param>
        public void EditAlias(string account, string newAlias)
        {
            var multiSig = MultiSignatureAccountMappings.FirstOrDefault(x => x.Address == account);
            
            if (multiSig != null)
            {
                multiSig.Alias = newAlias;
                OnStateChanged?.Invoke(this, new MultiSignatureAccountMappingStateChangedEventArgs(this));
            }
        }

        /// <summary>
        /// The mappings.
        /// </summary>
        public List<MultiSignatureAccountMapping> MultiSignatureAccountMappings
        {
            get; set;
        }

        /// <summary>
        /// An event raised whenever the state changes.
        /// </summary>
        public event EventHandler<MultiSignatureAccountMappingStateChangedEventArgs> OnStateChanged;
    }
}
