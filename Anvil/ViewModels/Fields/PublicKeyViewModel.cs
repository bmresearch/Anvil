using Anvil.Core.ViewModels;
using ReactiveUI;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;
using System;

namespace Anvil.ViewModels.Fields
{
    public class PublicKeyViewModel : ViewModelBase
    {
        public PublicKeyViewModel()
        {
            this.WhenAnyValue(x => x.PublicKeyString)
                .Subscribe(x => 
                {
                    if (PublicKey != null) 
                    { 
                        PublicKey = null;
                        Verified = false;
                    }
                    Input = !string.IsNullOrEmpty(x);
                    byte[] decoded;
                    try
                    {
                        decoded = Encoders.Base58.DecodeData(x);
                    }
                    catch (Exception ex)
                    {
                        Verified = false;
                        return;
                    }
                    if (decoded.Length != PublicKey.PublicKeyLength) 
                    { 
                        Verified = false;
                        return;
                    }
                    Verified = decoded.IsOnCurve();
                    if(Verified) PublicKey = new PublicKey(_publicKeyString);
                });
        }

        public void Clear()
        {
            PublicKeyString = string.Empty;
        }

        private bool _verified;
        public bool Verified
        {
            get => _verified;
            set => this.RaiseAndSetIfChanged(ref _verified, value);
        }

        private bool _input;
        public bool Input
        {
            get => _input;
            set => this.RaiseAndSetIfChanged(ref _input, value);
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
            set => this.RaiseAndSetIfChanged(ref _publicKeyString, value);
        }
    }
}
