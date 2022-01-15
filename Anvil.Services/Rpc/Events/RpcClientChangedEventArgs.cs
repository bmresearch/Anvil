using Solnet.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Rpc.Events
{
    /// <summary>
    /// An event raised whenever the rpc client changes.
    /// </summary>
    public class RpcClientChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new rpc client instance.
        /// </summary>
        public IRpcClient Client { get; init; }

        /// <summary>
        /// Initialize the event with the given client instance..
        /// </summary>
        /// <param name="client">The client instance.</param>
        public RpcClientChangedEventArgs(IRpcClient client)
        {
            Client = client;
        }
    }
}
