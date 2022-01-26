using Anvil.Services.Store.Events;
using Anvil.Services.Store.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anvil.Services.Store.State
{
    public class MultiSignatureAccountMappingState : State<List<MultiSignatureAccountMapping>>
    {
        public void AddMapping(MultiSignatureAccountMapping mapping)
        {
            Value.Add(mapping);
            OnStateChanged?.Invoke(this, new MultiSignatureAccountMappingStateChangedEventArgs(this));
        }

        public MultiSignatureAccountMapping GetMapping(PublicKey account)
        {
            return Value.FirstOrDefault(x => x.MultiSignature == account.Key);
        }

        public List<MultiSignatureAccountMapping> MultiSignatureAccountMappings
        {
            get => Value;
        }


        public event EventHandler<MultiSignatureAccountMappingStateChangedEventArgs> OnStateChanged;
    }
}
