using Anvil.Core.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels
{
    public class UnlockWalletViewModel : ViewModelBase
    {
        public UnlockWalletViewModel()
        {

        }

        private string _password;
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }
    }
}
