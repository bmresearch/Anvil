using Anvil.Core.ViewModels;
using Anvil.Models;
using ReactiveUI;
using System.Reactive;

namespace Anvil.ViewModels.Wallet
{
    public class UnlockWalletViewModel : ViewModelBase
    {
        public UnlockWalletViewModel()
        {
            var canConfirm = this.WhenAny(
                w => w.Password,
                (x) => !string.IsNullOrEmpty(x.Value));

            Confirm = ReactiveCommand.Create(
                () =>
                {
                    return new WalletUnlock
                    {
                        Password = Password
                    };
                }, canConfirm);
        }

        public void TriggerShowPassword()
        {
            ShowPassword = !ShowPassword;
        }

        private bool _showPassword;
        public bool ShowPassword
        {
            get => _showPassword;
            set => this.RaiseAndSetIfChanged(ref _showPassword, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => this.RaiseAndSetIfChanged(ref _isProcessing, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private bool _wrongPassword;
        public bool WrongPassword
        {
            get => _wrongPassword;
            set => this.RaiseAndSetIfChanged(ref _wrongPassword, value);
        }

        private string _progressStatus;
        public string ProgressStatus
        {
            get => _progressStatus;
            set
            {
                if (value.Contains("Wrong"))
                {
                    WrongPassword = true;
                }
                else
                {
                    WrongPassword = false;
                }
                this.RaiseAndSetIfChanged(ref _progressStatus, value);
            }
        }

        /// <summary>
        /// The command to navigate forward.
        /// </summary>
        public ReactiveCommand<Unit, WalletUnlock> Confirm { get; }
    }
}
