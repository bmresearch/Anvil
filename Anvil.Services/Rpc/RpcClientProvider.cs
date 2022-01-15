using Anvil.Services.Rpc.Events;
using Solnet.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services
{
    /// <summary>
    /// An <see cref="IRpcClient"/> provider which only exposes the client and provides functionality to easily reload it during runtime.
    /// </summary>
    public class RpcClientProvider : IRpcClientProvider
    {
        /// <summary>
        /// Initialize the <see cref="RpcClientProvider"/>.
        /// </summary>
        /// <param name="cluster">The RPC cluster to use.</param>
        public RpcClientProvider(Cluster cluster) => Load(cluster);

        /// <summary>
        /// Initialize the <see cref="RpcClientProvider"/>.
        /// </summary>
        /// <param name="url">The URL of the RPC.</param>
        public RpcClientProvider(string url) => Load(url);

        /// <inheritdoc cref="IRpcClientProvider.Load(Cluster)"/>
        public void Load(Cluster cluster)
        {
            Client = ClientFactory.GetClient(cluster);
            OnClientChanged?.Invoke(this, new RpcClientChangedEventArgs(Client));
        }

        /// <inheritdoc cref="IRpcClientProvider.Load(string)"/>
        public void Load(string url)
        {
            Client = ClientFactory.GetClient(url);
            OnClientChanged?.Invoke(this, new RpcClientChangedEventArgs(Client));
        }

        /// <inheritdoc cref="IRpcClientProvider.Client"/>
        public IRpcClient Client { get; private set; }

        /// <inheritdoc cref="IRpcClientProvider.OnClientChanged"/>
        public event EventHandler<RpcClientChangedEventArgs> OnClientChanged;
    }
}
