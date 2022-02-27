using Anvil.Services.Store.Events;
using Anvil.Services.Store.Models;
using System;
using System.Collections.Generic;

namespace Anvil.Services
{
    /// <summary>
    /// Specifies functionality for a <see cref="WatchOnlyAccount"/> store.
    /// </summary>
    public interface IWatchOnlyAccountStore
    {
        /// <summary>
        /// Adds a new <see cref="WatchOnlyAccount"/> to the store.
        /// </summary>
        /// <param name="account">The account to add.</param>
        void AddAccount(WatchOnlyAccount account);

        /// <summary>
        /// Edits the alias of the given account.
        /// </summary>
        /// <param name="account">The account to edit.</param>
        /// <param name="newAlias">The new alias.</param>
        void EditAlias(string account, string newAlias);

        /// <summary>
        /// The existing <see cref="WatchOnlyAccount"/>s.
        /// </summary>
        List<WatchOnlyAccount> WatchOnlyAccounts { get; }

        /// <summary>
        /// An event raised whenever the store state changes.
        /// </summary>
        event EventHandler<WatchOnlyAccountStateChangedEventArgs> OnStateChanged;
    }
}
