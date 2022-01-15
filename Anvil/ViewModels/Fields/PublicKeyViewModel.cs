using Anvil.Core.ViewModels;
using ReactiveUI;
using Solnet.Rpc.Utilities;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Fields
{
    public class PublicKeyViewModel : ViewModelBase
    {
        public PublicKeyViewModel()
        {
            IsValid = this.WhenAnyValue(
                x => x.PublicKey,
                y => y.PublicKeyString,
                (x, y) => x != null && !string.IsNullOrEmpty(y));
        }

        public IObservable<bool> IsValid 
        {
            get; init; 
        }

        private PublicKey _publicKey;
        public PublicKey PublicKey
        {
            get => _publicKey;
            set => this.RaiseAndSetIfChanged(ref _publicKey, value);
        }

        private string _publicKeyString = string.Empty;
        public string PublicKeyString
        {
            get => _publicKeyString;
            set
            {
                if (_publicKey != null) PublicKey = null;
                byte[] decoded;
                try
                {
                    decoded = Encoders.Base58.DecodeData(value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception trying to decode address: {ex.Message}");
                    return;
                }

                if (Ed25519Extensions.IsOnCurve(decoded))
                {
                    PublicKey = new PublicKey(_publicKeyString);
                }
                this.RaiseAndSetIfChanged(ref _publicKeyString, value);
            }
        }
    }
}
