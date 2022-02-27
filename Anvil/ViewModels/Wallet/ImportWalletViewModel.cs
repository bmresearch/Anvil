using Anvil.Core.ViewModels;
using Anvil.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Solnet.Wallet.Bip39;
using System;
using System.Reactive;

namespace Anvil.ViewModels.Wallet
{
    public class ImportWalletViewModel : ViewModelBase
    {
        private ApplicationState _appState;

        public ImportWalletViewModel(ApplicationState appState)
        {
            _appState = appState;

            var canConfirm = this.WhenAny(
                x => x.Mnemonic,
                y => y.PrivateKeyFilePath,
                w => w.Password,
                z => z.ConfirmPassword,
                (x,y,w,z) => (x.Value != null || !string.IsNullOrEmpty(y.Value)) && w.Value == z.Value);

            Confirm = ReactiveCommand.Create(
                () =>
                {
                    return new WalletImport
                    {
                        Mnemonic = MnemonicString,
                        PrivateKeyFilePath = PrivateKeyFilePath,
                        Password = Password,
                        Alias = Alias,
                    };
                }, canConfirm);
        }

        public void ClearPrivateKeyFilePath()
        {
            PrivateKeyFilePath = string.Empty;
        }

        public async void OpenPrivateKeyFileSelection()
        {
            var ofd = new OpenFileDialog() 
            { 
                AllowMultiple = false,
                Title = "Select Private Key File"
            }; 
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var selected = await ofd.ShowAsync(desktop.MainWindow);
                if (selected == null) return;
                if (selected.Length > 0)
                {
                    PrivateKeyFilePath = selected[0];
                }
            }
        }

        private void ValidateMnemonicString()
        {
            if (string.IsNullOrEmpty(_mnemonicString)) 
            {
                MnemonicValidationError = false;
                return; 
            }
            try
            {
                Mnemonic = new Mnemonic(_mnemonicString, WordList.AutoDetect(_mnemonicString));
                if (Mnemonic.IsValidChecksum)
                {
                    MnemonicValidationError = false;
                    return;
                }
            } catch(Exception)
            {
                MnemonicValidationError = true;
            }
        }

        private bool _mnemonicValidationError;
        public bool MnemonicValidationError
        {
            get => _mnemonicValidationError;
            set => this.RaiseAndSetIfChanged(ref _mnemonicValidationError, value);
        }

        private Mnemonic _mnemonic;
        public Mnemonic Mnemonic
        {
            get => _mnemonic;
            set => this.RaiseAndSetIfChanged(ref _mnemonic, value);
        }

        private string _privateKeyFilePath;
        public string PrivateKeyFilePath
        {
            get => _privateKeyFilePath;
            set => this.RaiseAndSetIfChanged(ref _privateKeyFilePath, value);
        }

        private string _mnemonicString;
        public string MnemonicString
        {
            get => _mnemonicString;
            set 
            {
                this.RaiseAndSetIfChanged(ref _mnemonicString, value);
                ValidateMnemonicString();
            }
        }

        private string _alias;
        public string Alias
        {
            get => _alias;
            set => this.RaiseAndSetIfChanged(ref _alias, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }
        

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
        }

        /// <summary>
        /// The command to navigate forward.
        /// </summary>
        public ReactiveCommand<Unit, WalletImport> Confirm { get; }
    }
}
