using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Network.Events
{
    /// <summary>
    /// An event raised whenever the network connection status changes.
    /// </summary>
    public class NetworkConnectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Whether there's a network connection.
        /// </summary>
        public bool Connected { get; init; }

        /// <summary>
        /// Initialize the event with the given connection status.
        /// </summary>
        /// <param name="connected">Whether there's a network connection.</param>
        public NetworkConnectionChangedEventArgs(bool connected)
        {
            Connected = connected;
        }
    }
}
