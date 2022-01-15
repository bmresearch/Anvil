using Anvil.Core.ViewModels;
using ReactiveUI;
using Solnet.Wallet;
using System;

namespace Anvil.ViewModels.Fields
{
    public class SignatureWrapperViewModel : ViewModelBase
    {
        private byte[] _message;

        public SignatureWrapperViewModel() { }

        /// <summary>
        /// Initialize the signature wrapper view model with the corresponding public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        public SignatureWrapperViewModel(PublicKey publicKey, byte[] message)
        {
            _message = message;
            PublicKey = publicKey;
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

        private string _signature;
        public string Signature
        {
            get => _signature;
            set
            {
                this.RaiseAndSetIfChanged(ref _signature, value);
                Input = !string.IsNullOrEmpty(value);

                byte[] signature;
                try
                {
                    signature = Convert.FromBase64String(value);
                    if(signature.Length != 64)
                    {
                        Verified = false;
                        return;
                    }
                }
                catch (Exception e)
                {
                    Verified = false;
                    return;
                }
                Verified = PublicKey.Verify(_message, signature);
            }
        }
    }
}
