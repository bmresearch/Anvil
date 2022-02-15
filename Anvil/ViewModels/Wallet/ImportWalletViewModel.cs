using Anvil.Core.ViewModels;
using Anvil.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
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
                (x,y,w,z) => (!string.IsNullOrEmpty(x.Value) && w.Value == z.Value) || !string.IsNullOrEmpty(y.Value));

            Confirm = ReactiveCommand.Create(
                () =>
                {
                    return new WalletImport
                    {
                        Mnemonic = Mnemonic,
                        PrivateKeyFilePath = PrivateKeyFilePath,
                        Password = Password
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

        private string _privateKeyFilePath;
        public string PrivateKeyFilePath
        {
            get => _privateKeyFilePath;
            set => this.RaiseAndSetIfChanged(ref _privateKeyFilePath, value);
        }

        private string _mnemonic;
        public string Mnemonic
        {
            get => _mnemonic;
            set => this.RaiseAndSetIfChanged(ref _mnemonic, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }
        

        private string _confirmPassword;
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
