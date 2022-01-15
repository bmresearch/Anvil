using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.Services.Wallet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anvil.Crafter.ViewModels
{
    public class TransactionSignViewModel : ViewModelBase
    {
        private IWalletService _walletService;
        public string Header => "Sign Transaction";


        public TransactionSignViewModel(IWalletService walletService)
        {
            _walletService = walletService;
        }
    }
}
