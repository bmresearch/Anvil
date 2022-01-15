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
        private bool _isEncrypted;
        public bool IsEncrypted
        {
            get => _isEncrypted;
            set => this.RaiseAndSetIfChanged(ref _isEncrypted, value);
        }

        [DataMember]
        private bool _mnemonicSaved;
        public bool MnemonicSaved 
        { 
            get => _mnemonicSaved;
            set => this.RaiseAndSetIfChanged(ref _mnemonicSaved, value);
        }

        [DataMember]
        private string _mnemonicStoreFilePath = string.Empty;
        public string MnemonicStoreFilePath
        {
            get => _mnemonicStoreFilePath;
            set => this.RaiseAndSetIfChanged(ref _mnemonicStoreFilePath, value);
        }

        [DataMember]
        private string _privateKeyFilePath = string.Empty;
        public string PrivateKeyFilePath 
        { 
            get => _privateKeyFilePath;
            set => this.RaiseAndSetIfChanged(ref _privateKeyFilePath, value); 
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

        public bool WalletExists => PrivateKeyFilePath != string.Empty && MnemonicSaved;
    }
}
