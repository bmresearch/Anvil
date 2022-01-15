using Anvil.Core.ViewModels;
using Anvil.Services;
using Solnet.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Crafter.ViewModels
{
    public class CrafterViewModel : ViewModelBase
    {
        public string Header => "Crafter";

        public List<ViewModelBase> Tabs { get; set; }


        public CrafterViewModel(IRpcClient rpcClient, IWalletService walletService)
        {
            Tabs = new List<ViewModelBase>()
            {
                new TransactionCraftViewModel(rpcClient),
                new TransactionSignViewModel(walletService),
                new TransactionSendViewModel(rpcClient)
            };
        }
    }
}
