using Anvil.Core.ViewModels;
using Anvil.Services.Store.Models;
using ReactiveUI;
using Solnet.Programs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Crafter
{
    public class NonceAccountViewModel : ViewModelBase
    {
        public NonceAccountViewModel(NonceAccount nonceAccount, NonceAccountMapping mapping)
        {
            NonceAccount = nonceAccount;
            Nonce = nonceAccount.Nonce.Key;
            NonceAccountMap = mapping;
        }

        private NonceAccount _nonceAccount;
        public NonceAccount NonceAccount
        {
            get => _nonceAccount;
            set => this.RaiseAndSetIfChanged(ref _nonceAccount, value);
        }

        private string _nonce;
        public string Nonce
        {
            get => _nonce;
            set => this.RaiseAndSetIfChanged(ref _nonce, value);
        }

        private NonceAccountMapping _nonceAccountMap;
        public NonceAccountMapping NonceAccountMap
        {
            get => _nonceAccountMap;
            set => this.RaiseAndSetIfChanged(ref _nonceAccountMap, value);
        }
    }
}
