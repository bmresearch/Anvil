using Anvil.Core.ViewModels;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Rpc;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anvil.Crafter.ViewModels
{
    public class TransactionSendViewModel : ViewModelBase
    {
        private IRpcClient _rpcClient;

        public string Header => "Send Transaction";

        public TransactionSendViewModel(IRpcClient rpcClient)
        {
            _rpcClient = rpcClient;

            /*
            ulong minBalanceForExemptionAcc =
                _rpcClient.GetMinimumBalanceForRentExemption(NonceAccount.AccountDataSize).Result;
            var nonceAccount = new Account();
            SystemProgram.CreateAccount(thisAccount, nonceAccount, minBalanceForExemptionAcc, NonceAccount.AccountDataSize,
                    SystemProgram.ProgramIdKey);
            SystemProgram.InitializeNonceAccount(nonceAccount, offlineAccount);
            */
        }
    }
}
