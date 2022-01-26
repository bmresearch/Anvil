using Anvil.Services.Rpc.Events;
using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        /// <inheritdoc cref="IRpcClientProvider.PollTxAsync(string,Commitment)"/>
        public async Task<TransactionMetaSlotInfo> PollTxAsync(string signature, Commitment commitment)
        {
            RequestResult<TransactionMetaSlotInfo> txMeta = await Client.GetTransactionAsync(signature);
            while (!txMeta.WasSuccessful)
            {
                await Task.Delay(1000);
                txMeta = await Client.GetTransactionAsync(signature);
                if (txMeta.WasSuccessful)
                    return txMeta.Result;
            }
            return txMeta.Result;
        }

        /// <inheritdoc cref="IRpcClientProvider.Client"/>
        public IRpcClient Client { get; private set; }

        /// <inheritdoc cref="IRpcClientProvider.OnClientChanged"/>
        public event EventHandler<RpcClientChangedEventArgs> OnClientChanged;
    }
}
