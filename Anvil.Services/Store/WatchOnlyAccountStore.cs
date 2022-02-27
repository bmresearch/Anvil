using Anvil.Services.Store.Config;
using Anvil.Services.Store.Events;
using Anvil.Services.Store.Models;
using Anvil.Services.Store.State;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Anvil.Services.Store
{
    /// <summary>
    /// Implements functionality for a <see cref="WatchOnlyAccount"/> store.
    /// </summary>
    public class WatchOnlyAccountStore : Abstract.Store, IWatchOnlyAccountStore
    {
        /// <summary>
        /// The state of the nonce account mapping store.
        /// </summary>
        private WatchOnlyAccountState _state;

        /// <summary>
        /// Initialize the <see cref="NonceAccountMappingStore"/> with the given <see cref="ILogger"/> and <see cref="StoreConfig"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The store config.</param>
        public WatchOnlyAccountStore(ILogger logger, StoreConfig config) : base(logger, config)
        {
            _state = _persistenceDriver.LoadState<WatchOnlyAccountState>();
            if (_state.WatchOnlyAccounts == null)
            {
                _state.WatchOnlyAccounts = new();
                _persistenceDriver.SaveState(_state);
            }
            _state.OnStateChanged += _state_OnStateChanged;
        }

        /// <summary>
        /// Handle changes to the state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void _state_OnStateChanged(object sender, WatchOnlyAccountStateChangedEventArgs e)
        {
            _persistenceDriver.SaveState(e.State);
            OnStateChanged?.Invoke(this, e);
        }

        /// <inheritdoc cref="IWatchOnlyAccountStore.WatchOnlyAccounts"/>
        public List<WatchOnlyAccount> WatchOnlyAccounts 
        { 
            get => _state.WatchOnlyAccounts;
        }

        /// <inheritdoc cref="IWatchOnlyAccountStore.AddAccount(WatchOnlyAccount)"/>
        public void AddAccount(WatchOnlyAccount account)
        {
            _state.AddAccount(account);
        }

        /// <inheritdoc cref="IWatchOnlyAccountStore.AddAccount(string, string)"/>
        public void EditAlias(string account, string newAlias)
        {
            _state.EditAlias(account, newAlias);
        }

        /// <inheritdoc cref="IWatchOnlyAccountStore.OnStateChanged"/>
        public event EventHandler<WatchOnlyAccountStateChangedEventArgs> OnStateChanged;

        /// <summary>
        /// The store file name.
        /// </summary>
        public const string FileName = "watchonlyaccounts.store";

    }
}
