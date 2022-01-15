using Anvil.Core.ViewModels;
using Anvil.Services;
using Solnet.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Crafter
{
    public class CrafterViewModel : ViewModelBase
    {
        private IRpcClientProvider _rpcProvider;
        private IWalletService _walletService;
        private INonceAccountMappingStore _nonceAccountMappingStore;

        public string Header => "Crafter";

        public List<ViewModelBase> Tabs { get; set; }

        public CrafterViewModel(IRpcClientProvider rpcProvider, IWalletService walletService, INonceAccountMappingStore nonceAccountMappingStore)
        {
            _rpcProvider = rpcProvider;
            _walletService = walletService;
            _nonceAccountMappingStore = nonceAccountMappingStore;

            Tabs = new List<ViewModelBase>()
            {
                new TransactionCraftViewModel(rpcProvider, nonceAccountMappingStore),
                new TransactionSignViewModel(walletService),
                new TransactionSendViewModel(rpcProvider)
            };
        }
    }
}
