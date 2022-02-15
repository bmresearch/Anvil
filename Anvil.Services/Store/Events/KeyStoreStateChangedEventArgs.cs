using Anvil.Services.Store.State;
using System;

namespace Anvil.Services.Store.Events
{
    public class KeyStoreStateChangedEventArgs : EventArgs
    {
        public KeyStoreState State;

        public KeyStoreStateChangedEventArgs(KeyStoreState state)
        {
            State = state;
        }
    }
}
