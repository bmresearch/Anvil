using Anvil.Services.Store.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Store.Events
{
    public class MultiSignatureAccountMappingStateChangedEventArgs
    {
        public MultiSignatureAccountMappingState State;

        public MultiSignatureAccountMappingStateChangedEventArgs(MultiSignatureAccountMappingState state)
        {
            State = state;
        }
    }
}
