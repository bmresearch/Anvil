using Anvil.Services.Store.Config;
using Anvil.Services.Store.Models;
using Anvil.Services.Store.State;
using Microsoft.Extensions.Logging;
using Solnet.Wallet;
using System.Collections.Generic;

namespace Anvil.Services.Store
{
    /// <summary>
    /// Implements a store for <see cref="NonceAccountMapping"/>.
    /// </summary>
    public class NonceAccountMappingStore : Abstract.Store, INonceAccountMappingStore
    {
        /// <summary>
        /// The state of the nonce account mapping store.
        /// </summary>
        private NonceAccountMappingState _state;

        /// <summary>
        /// Initialize the <see cref="NonceAccountMappingStore"/> with the given <see cref="ILogger"/> and <see cref="StoreConfig"/>.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public NonceAccountMappingStore(ILogger logger, StoreConfig config) : base(logger, config)
        {
            _state = _persistenceDriver.LoadState<NonceAccountMappingState>();
            if (_state.NonceAccountMappings == null)
            {
                _state.NonceAccountMappings = new();
                _persistenceDriver.SaveState(_state);
            }
            _state.OnStateChanged += _state_OnStateChanged;
        }

        /// <summary>
        /// Handle changes to the state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void _state_OnStateChanged(object sender, Events.NonceAccountMappingStateChangedEventArgs e)
        {
            _persistenceDriver.SaveState(e.State);
        }

        /// <inheritdoc cref="INonceAccountMappingStore.AddMapping(NonceAccountMapping)"></inheritdoc>
        public void AddMapping(NonceAccountMapping mapping)
        {
            _state.AddMapping(mapping);
        }

        /// <inheritdoc cref="INonceAccountMappingStore.GetMapping(PublicKey)"></inheritdoc>
        public NonceAccountMapping GetMapping(PublicKey authority)
        {
            return _state.GetMapping(authority);
        }

        /// <inheritdoc cref="INonceAccountMappingStore.NonceAccountMappings"></inheritdoc>
        public List<NonceAccountMapping> NonceAccountMappings 
        { 
            get => _state.NonceAccountMappings;
        }

        public const string FileName = "nonceaccount.store";
    }
}
