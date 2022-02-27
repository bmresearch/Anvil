using Anvil.Services.Store.State;
using System;

namespace Anvil.Services.Store.Events
{
    /// <summary>
    /// The event args for when the <see cref="KeyStore"/> state changes.
    /// </summary>
    public class KeyStoreStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The latest state.
        /// </summary>
        public KeyStoreState State;

        /// <summary>
        /// Initialize the event args with the given <see cref="KeyStoreState"/>.
        /// </summary>
        /// <param name="state">The latest state.</param>
        public KeyStoreStateChangedEventArgs(KeyStoreState state)
        {
            State = state;
        }
    }
}
