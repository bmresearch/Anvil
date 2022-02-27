using Anvil.Core.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Solnet.Wallet;
using System;
using System.IO;

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

            this.WhenAnyValue(x => x.Signature)
                .Subscribe(x =>
                {
                    Input = !string.IsNullOrEmpty(x);

                    byte[] signature;
                    try
                    {
                        signature = Convert.FromBase64String(x);
                        if (signature.Length != 64)
                        {
                            Verified = false;
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        Verified = false;
                        return;
                    }
                    Verified = PublicKey.Verify(_message, signature);
                });
        }

        public async void LoadSignatureFromFile()
        {
            var ofd = new OpenFileDialog()
            {
                AllowMultiple = false,
                Title = "Select Signature File",
                Filters = new()
                {
                    new FileDialogFilter()
                    {
                        Name = "*",
                        Extensions = new() { "sig" }
                    }
                }
            };
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var selected = await ofd.ShowAsync(desktop.MainWindow);
                if (selected == null) return;
                if (selected.Length > 0)
                {
                    if (!File.Exists(selected[0])) return;

                    Signature = await File.ReadAllTextAsync(selected[0]);
                }
            }
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
            set => this.RaiseAndSetIfChanged(ref _signature, value);
        }

        public string ShortenedPublicKey
        {
            get => _publicKey.Key[..6] + "..." + _publicKey.Key[^6..];
        }
    }
}
