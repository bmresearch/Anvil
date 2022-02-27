using Anvil.Services.Store.State;
using System;

namespace Anvil.Services.Store.Events
{
    /// <summary>
    /// The event args for when the <see cref="WatchOnlyAccountStore"/> state changes.
    /// </summary>
    public class WatchOnlyAccountStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The latest state.
        /// </summary>
        public WatchOnlyAccountState State;

        /// <summary>
        /// Initialize the event args with the given state.
        /// </summary>
        /// <param name="state">The latest state.</param>
        public WatchOnlyAccountStateChangedEventArgs(WatchOnlyAccountState state)
        {
            State = state;
        }
    }
}
