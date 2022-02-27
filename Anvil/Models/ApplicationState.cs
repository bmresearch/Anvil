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
        public Cluster Cluster
        {
            get => _cluster;
            set => this.RaiseAndSetIfChanged(ref _cluster, value);
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
