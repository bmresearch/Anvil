using System;

namespace Anvil.Services.Store.Events
{
    /// <summary>
    /// The event args of when the <see cref="KeyStore"/> has an update.
    /// <remarks>
    /// This is only used for the <see cref="KeyStore"/> to raise an event which will signal the service to encrypt the latest store state.
    /// </remarks>
    /// </summary>
    public class KeyStoreUpdateEventArgs : EventArgs
    {
        public KeyStoreUpdateEventArgs()
        {
        }
    }
}
