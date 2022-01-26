using Anvil.Core.Modules;
using Anvil.Services.Store.Config;
using Anvil.Services.Store.Models;
using Anvil.Services.Store.State;
using Microsoft.Extensions.Logging;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Store
{
    /// <summary>
    /// Implements a store for <see cref="NonceAccountMapping"/>.
    /// </summary>
    public class NonceAccountMappingStore : Store, INonceAccountMappingStore
    {
        private NonceAccountMappingState _state;

        /// <summary>
        /// 
        /// </summary>
        public NonceAccountMappingStore(ILogger logger, StoreConfig config) : base(logger, config)
        {
            _state = _persistenceDriver.LoadState<NonceAccountMappingState>();
            _state.PropertyChanged += (s, a) => {
                _persistenceDriver.SaveState(_state); 
            };
            if (_state.Value == null)
            {
                _state.Value = new();
                _persistenceDriver.SaveState(_state);
            }
            _state.OnStateChanged += _state_OnStateChanged;
        }

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

        public const string FileName = "nonce_accounts.map";
    }
}
