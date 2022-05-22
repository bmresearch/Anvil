using ReactiveUI;
using Solnet.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Models
{
    [DataContract]
    public class ApplicationState : ReactiveObject
    {
        [DataMember]
        private string _keyStoreFilePath = AppContext.BaseDirectory;
        public string KeyStoreFilePath
        {
            get => _keyStoreFilePath;
            set => this.RaiseAndSetIfChanged(ref _keyStoreFilePath, value);
        }

        [DataMember]
        private string _rpcUrl = string.Empty;
        public string RpcUrl
        {
            get => _rpcUrl;
            set => this.RaiseAndSetIfChanged(ref _rpcUrl, value);
        }


        [DataMember]
        private Cluster _cluster = Cluster.MainNet;
        
        /// <summary>
        /// A setting used in order to choose which cluster to use as an RPC Client Provider.
        /// </summary>
        public Cluster Cluster
        {
            get => _cluster;
            set => this.RaiseAndSetIfChanged(ref _cluster, value);
        }
        
        [DataMember]
        private Cluster _network = Cluster.MainNet;

        /// <summary>
        /// A flag used in order to tag the current network according to the received genesis hash.
        /// </summary>
        public Cluster Network
        {
            get => _network;
            set => this.RaiseAndSetIfChanged(ref _network, value);
        }

        [DataMember]
        private string _storePath = AppContext.BaseDirectory;
        public string StorePath
        {
            get => _storePath;
            set => this.RaiseAndSetIfChanged(ref _storePath, value);
        }
    }
}
