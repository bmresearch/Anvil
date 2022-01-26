using Anvil.Services.Store.Events;
using Anvil.Services.Store.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anvil.Services.Store.State
{
    public class NonceAccountMappingState : State<List<NonceAccountMapping>>
    {
        public void AddMapping(NonceAccountMapping mapping)
        {
            Value.Add(mapping);
            OnStateChanged?.Invoke(this, new NonceAccountMappingStateChangedEventArgs(this));
        }

        public NonceAccountMapping GetMapping(PublicKey account)
        {
            return Value.FirstOrDefault(x => x.Authority == account.Key);
        }

        public List<NonceAccountMapping> NonceAccountMappings
        {
            get => Value;
        }

        public event EventHandler<NonceAccountMappingStateChangedEventArgs> OnStateChanged;
    }
}
