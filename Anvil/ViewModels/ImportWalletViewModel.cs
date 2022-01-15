using Anvil.Core.ViewModels;
using Anvil.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System.Reactive;

namespace Anvil.ViewModels
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
                (x,y) => !string.IsNullOrEmpty(x.Value) || !string.IsNullOrEmpty(y.Value));

            Confirm = ReactiveCommand.Create(
                () =>
                {
                    return new WalletImport
                    {
                        Mnemonic = Mnemonic,
                        PrivateKeyFilePath = PrivateKeyFilePath,
                        SaveMnemonic = SaveMnemonic,
                        MnemonicStorePath = MnemonicStorePath,
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

        public async void OpenStorePathSelection()
        {
            var ofd = new OpenFolderDialog()
            {
                Title = "Select Key Store Path"
            };
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var selected = await ofd.ShowAsync(desktop.MainWindow);
                if (selected == null) return;
                MnemonicStorePath = selected;
            }
        }

        private string _mnemonicStorePath;
        public string MnemonicStorePath
        {
            get => _mnemonicStorePath;
            set => this.RaiseAndSetIfChanged(ref _mnemonicStorePath, value);
        }

        private string _privateKeyFilePath;
        public string PrivateKeyFilePath
        {
            get => _privateKeyFilePath;
            set => this.RaiseAndSetIfChanged(ref _privateKeyFilePath, value);
        }

        private bool _saveMnemonic = false;
        public bool SaveMnemonic
        {
            get => _saveMnemonic;
            set 
            {
                this.RaiseAndSetIfChanged(ref _saveMnemonic, value);
                if (!value)
                {
                    MnemonicStorePath = string.Empty;
                    Password = string.Empty;
                }
            }
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

        /// <summary>
        /// The command to navigate forward.
        /// </summary>
        public ReactiveCommand<Unit, WalletImport> Confirm { get; }
    }
}
