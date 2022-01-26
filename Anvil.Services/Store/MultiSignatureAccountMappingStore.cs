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
    /// Implements a store for <see cref="MultiSignatureMapping"/>.
    /// </summary>
    public class MultiSignatureAccountMappingStore : Store, IMultiSignatureAccountMappingStore
    {
        private MultiSignatureAccountMappingState _state;

        /// <summary>
        /// 
        /// </summary>
        public MultiSignatureAccountMappingStore(ILogger logger, StoreConfig config) : base(logger, config)
        {
            _state = _persistenceDriver.LoadState<MultiSignatureAccountMappingState>();
            _state.PropertyChanged += (s, a) => { _persistenceDriver.SaveState(_state); };
            if(_state.Value == null)
            {
                _state.Value = new();
                _persistenceDriver.SaveState(_state);
            }
            _state.OnStateChanged += _state_OnStateChanged;
        }

        private void _state_OnStateChanged(object sender, Events.MultiSignatureAccountMappingStateChangedEventArgs e)
        {
            _persistenceDriver.SaveState(e.State);
        }

        /// <inheritdoc cref="IMultiSignatureAccountMappingStore.AddMapping(MultiSignatureAccountMapping)"
        public void AddMapping(MultiSignatureAccountMapping mapping)
        {
            _state.AddMapping(mapping);
        }

        /// <inheritdoc cref="IMultiSignatureAccountMappingStore.GetMapping(PublicKey)"
        public MultiSignatureAccountMapping GetMapping(PublicKey account)
        {
            return _state.GetMapping(account);
        }

        /// <inheritdoc cref="IMultiSignatureAccountMappingStore.MultiSignatureAccountMappings"
        public List<MultiSignatureAccountMapping> MultiSignatureAccountMappings 
        { 
            get => _state.MultiSignatureAccountMappings; 
        }

        public const string FileName = "multisig_accounts.map";
    }
}
