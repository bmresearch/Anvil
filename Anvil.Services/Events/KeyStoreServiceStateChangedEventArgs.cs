using Anvil.Services.Enums;

namespace Anvil.Services.Events
{
    /// <summary>
    /// Represents a change in the <see cref="KeyStoreService"/> state.
    /// </summary>
    public class KeyStoreServiceStateChangedEventArgs
    {
        /// <summary>
        /// The new selected wallet.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The key store service state.
        /// </summary>
        public KeyStoreServiceState State { get; }

        /// <summary>
        /// Initialize the <see cref="KeyStoreServiceStateChangedEventArgs"/> with the given <see cref="KeyStoreServiceState"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="state">The key store state.</param>
        public KeyStoreServiceStateChangedEventArgs(string message, KeyStoreServiceState state)
        {
            Message = message;
            State = state;
        }
    }
}
