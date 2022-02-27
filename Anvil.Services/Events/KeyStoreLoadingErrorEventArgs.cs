using System;
using System.Collections.Generic;

namespace Anvil.Services.Events
{
    /// <summary>
    /// The event args of when the key store service had an error while loading.
    /// </summary>
    public class KeyStoreLoadingErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The paths of the unloaded wallets.
        /// </summary>
        public List<string> UnloadedWallets { get; set; }

        /// <summary>
        /// Initialize the
        /// </summary>
        /// <param name="unloaded"></param>
        public KeyStoreLoadingErrorEventArgs(List<string> unloaded)
        {
            UnloadedWallets = unloaded;
        }
    }
}
