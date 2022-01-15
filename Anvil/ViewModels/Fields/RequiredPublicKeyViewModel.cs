using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Fields
{
    public class RequiredPublicKeyViewModel : PublicKeyViewModel
    {
        public RequiredPublicKeyViewModel(bool isRequired)
        {
            _isRequired = isRequired;
        }

        private bool _isRequired;
        public bool IsRequired
        {
            get => _isRequired;
            set => this.RaiseAndSetIfChanged(ref _isRequired, value);
        }
    }
}
