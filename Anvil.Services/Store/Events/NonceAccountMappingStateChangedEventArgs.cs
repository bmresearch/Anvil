using Anvil.Services.Store.State;

namespace Anvil.Services.Store.Events
{
    public class NonceAccountMappingStateChangedEventArgs
    {
        public NonceAccountMappingState State;

        public NonceAccountMappingStateChangedEventArgs(NonceAccountMappingState state)
        {
            State = state;
        }
    }
}
