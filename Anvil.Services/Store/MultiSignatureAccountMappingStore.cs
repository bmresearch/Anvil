using Anvil.Services.Store.Config;
using Anvil.Services.Store.Events;
using Anvil.Services.Store.Models;
using Anvil.Services.Store.State;
using Microsoft.Extensions.Logging;
using Solnet.Wallet;
using System;
using System.Collections.Generic;

namespace Anvil.Services.Store
{
    /// <summary>
    /// Implements a store for <see cref="MultiSignatureAccountMapping"/>.
    /// </summary>
    public class MultiSignatureAccountMappingStore : Abstract.Store, IMultiSignatureAccountMappingStore
    {
        /// <summary>
        /// The store's state.
        /// </summary>
        private MultiSignatureAccountMappingState _state;

        /// <summary>
        /// Initialize the <see cref="MultiSignatureAccountMappingStore"/> with the given <see cref="ILogger"/> and <see cref="StoreConfig"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The config.</param>
        public MultiSignatureAccountMappingStore(ILogger logger, StoreConfig config) : base(logger, config)
        {
            _state = _persistenceDriver.LoadState<MultiSignatureAccountMappingState>();
            if(_state.MultiSignatureAccountMappings == null)
            {
                _state.MultiSignatureAccountMappings = new();
                _persistenceDriver.SaveState(_state);
            }
            _state.OnStateChanged += _state_OnStateChanged;
        }

        /// <summary>
        /// Handle changes to the state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void _state_OnStateChanged(object sender, MultiSignatureAccountMappingStateChangedEventArgs e)
        {
            _persistenceDriver.SaveState(e.State);
            OnStateChanged?.Invoke(this, e);
        }

        /// <inheritdoc cref="IMultiSignatureAccountMappingStore.AddMapping(MultiSignatureAccountMapping)"
        public void AddMapping(MultiSignatureAccountMapping mapping)
        {
            _state.AddMapping(mapping);
        }

        /// <inheritdoc cref="IMultiSignatureAccountMappingStore.EditAlias(string, string)"
        public void EditAlias(string account, string newAlias)
        {
            _state.EditAlias(account, newAlias);
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

        /// <inheritdoc cref="IMultiSignatureAccountMappingStore.OnStateChanged"
        public event EventHandler<MultiSignatureAccountMappingStateChangedEventArgs> OnStateChanged;

        /// <summary>
        /// The store file name.
        /// </summary>
        public const string FileName = "multisigaccount.store";

    }
}
