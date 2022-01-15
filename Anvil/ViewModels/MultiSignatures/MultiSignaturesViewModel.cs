using Anvil.Core.ViewModels;
using Anvil.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.MultiSignatures
{
    public class MultiSignaturesViewModel : ViewModelBase
    {
        private IRpcClientProvider _rpcProvider;
        private IWalletService _walletService;
        private IMultiSignatureAccountMappingStore _multiSigAccountMappingStore;

        public string Header => "Multi Signatures";

        public List<ViewModelBase> Tabs { get; set; }

        public MultiSignaturesViewModel(IRpcClientProvider rpcProvider, IWalletService walletService, IMultiSignatureAccountMappingStore multiSignatureAccountMappingStore)
        {
            _rpcProvider = rpcProvider;
            _walletService = walletService;
            _multiSigAccountMappingStore = multiSignatureAccountMappingStore;

            Tabs = new List<ViewModelBase>()
            {
                new MultiSignatureCreateViewModel(rpcProvider, walletService, multiSignatureAccountMappingStore),
                new MultiSignatureListViewModel(rpcProvider, multiSignatureAccountMappingStore)
            };
        }

    }
}
