using Anvil.Services.Rpc.Events;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using System;
using System.Threading.Tasks;

namespace Anvil.Services
{
    /// <summary>
    /// Specifies functionality for an <see cref="IRpcClient"/> provider.
    /// </summary>
    public interface IRpcClientProvider
    {
        /// <summary>
        /// The client instance.
        /// </summary>
        IRpcClient Client { get; }

        /// <summary>
        /// Loads a new <see cref="IRpcClient"/> instance with the given <see cref="Cluster"/>.
        /// </summary>
        /// <param name="cluster">The cluster to connect to.</param>
        void Load(Cluster cluster);

        /// <summary>
        /// Loads a new <see cref="IRpcClient"/> instance with the given url.
        /// </summary>
        /// <param name="url">The RPC URL.</param>
        void Load(string url);

        /// <summary>
        /// Polls the rpc client until a transaction signature has been confirmed.
        /// </summary>
        /// <param name="signature">The first transaction signature.</param>
        /// <param name="commitment">The commitment.</param>
        /// <returns>The transaction.</returns>
        Task<TransactionMetaSlotInfo> PollTxAsync(string signature, Commitment commitment);

        /// <summary>
        /// An event raised whenever the current <see cref="IRpcClient"/> instance changes.
        /// </summary>
        event EventHandler<RpcClientChangedEventArgs> OnClientChanged;
    }
}