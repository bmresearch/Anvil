using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.Services.Network;
using Avalonia.Controls.ApplicationLifetimes;
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
        #region Framework

        private IClassicDesktopStyleApplicationLifetime _appLifetime;

        #endregion

        #region Services

        private IRpcClientProvider _rpcProvider;
        private IWalletService _walletService;
        private INonceAccountMappingStore _nonceAccountMappingStore;

        private InternetConnectionService _internetConnectionService;

        #endregion

        public string Header => "Crafter";

        public List<ViewModelBase> Tabs { get; set; }

        public CrafterViewModel(IClassicDesktopStyleApplicationLifetime appLifetime,
            InternetConnectionService internetConnectionService,
            IRpcClientProvider rpcProvider, IWalletService walletService,
            INonceAccountMappingStore nonceAccountMappingStore, AddressBookService addressBookService)
        {
            _appLifetime = appLifetime;
            _internetConnectionService = internetConnectionService;
            _rpcProvider = rpcProvider;
            _walletService = walletService;
            _nonceAccountMappingStore = nonceAccountMappingStore;

            Tabs = new List<ViewModelBase>()
            {
                new TransactionCraftViewModel(appLifetime, internetConnectionService, rpcProvider,
                walletService, nonceAccountMappingStore, addressBookService),
                new TransactionSignViewModel(appLifetime, walletService),
                new TransactionSendViewModel(appLifetime, internetConnectionService, rpcProvider)
            };
        }
    }
}
