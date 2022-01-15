using Anvil.Core.ViewModels;
using Anvil.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.MultiSignatures
{
    public class MultiSignatureListViewModel : ViewModelBase
    {        
        public string Header => "View MultiSigs";

        private IRpcClientProvider _rpcProvider;
        private IMultiSignatureAccountMappingStore _multiSigAccountMappingStore;

        public MultiSignatureListViewModel(IRpcClientProvider rpcClientProvider, IMultiSignatureAccountMappingStore multiSignatureAccountMappingStore)
        {
            _rpcProvider = rpcClientProvider;
            _multiSigAccountMappingStore = multiSignatureAccountMappingStore;
        }
    }
}
