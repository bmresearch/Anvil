using Anvil.Services.Store.Events;
using Anvil.Services.Store.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;

namespace Anvil.Services
{
    /// <summary>
    /// Specifies functionality for a <see cref="MultiSignatureAccountMapping"/> store.
    /// </summary>
    public interface IMultiSignatureAccountMappingStore
    {
        /// <summary>
        /// Adds a new <see cref="MultiSignatureAccountMapping"/> to the store.
        /// </summary>
        /// <param name="mapping">The mapping to add.</param>
        void AddMapping(MultiSignatureAccountMapping mapping);

        /// <summary>
        /// Edits the alias of the given account.
        /// </summary>
        /// <param name="account">The account to edit.</param>
        /// <param name="newAlias">The new alias.</param>
        void EditAlias(string account, string newAlias);

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

        /// <summary>
        /// An event raised whenever the store state changes.
        /// </summary>
        event EventHandler<MultiSignatureAccountMappingStateChangedEventArgs> OnStateChanged;
    }
}
