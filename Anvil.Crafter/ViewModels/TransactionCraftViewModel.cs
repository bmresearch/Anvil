using Anvil.Core.ViewModels;
using Solnet.Rpc;
using System.Collections.Generic;

namespace Anvil.Crafter.ViewModels
{
    public class TransactionCraftViewModel : ViewModelBase
    {
        private IRpcClient _rpcClient;
        public string Header => "Craft Transaction";

        public static readonly List<string> Assets = new List<string>() { };

        public TransactionCraftViewModel(IRpcClient rpcClient)
        {
            _rpcClient = rpcClient;
        }

        public float AssetAmount { get; set; }

        public string SourceAccount { get; set; }

        public string DestinationAccount { get; set; }

        public int SelectedAssetIndex { get; set; }

        public string SelectedAsset { get; set; }
    }
}
