using Anvil.Services.Store.Events;
using Anvil.Services.Store.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anvil.Services.Store.State
{
    /// <summary>
    /// The <see cref="WatchOnlyAccountState"/> state.
    /// </summary>
    public class WatchOnlyAccountState
    {
        /// <summary>
        /// Add a new account.
        /// </summary>
        /// <param name="account">The account to add.</param>
        public void AddAccount(WatchOnlyAccount account)
        {
            WatchOnlyAccounts.Add(account);
            OnStateChanged?.Invoke(this, new WatchOnlyAccountStateChangedEventArgs(this));
        }

        /// <summary>
        /// Edits the alias of a given multi signature account.
        /// </summary>
        /// <param name="account">The multi signature account.</param>
        /// <param name="newAlias">The new alias.</param>
        public void EditAlias(string account, string newAlias)
        {
            var watchOnly = WatchOnlyAccounts.FirstOrDefault(x => x.Address == account);

            if (watchOnly != null)
            {
                watchOnly.Alias = newAlias;
                OnStateChanged?.Invoke(this, new WatchOnlyAccountStateChangedEventArgs(this));
            }
        }

        /// <summary>
        /// The mappings.
        /// </summary>
        public List<WatchOnlyAccount> WatchOnlyAccounts
        {
            get; set;
        }

        /// <summary>
        /// An event raised whenever a new account is added.
        /// </summary>
        public event EventHandler<WatchOnlyAccountStateChangedEventArgs> OnStateChanged;

    }
}
