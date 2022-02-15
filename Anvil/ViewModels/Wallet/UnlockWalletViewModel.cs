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

        private string _progressStatus;
        public string ProgressStatus
        {
            get => _progressStatus;
            set => this.RaiseAndSetIfChanged(ref _progressStatus, value);
        }

        /// <summary>
        /// The command to navigate forward.
        /// </summary>
        public ReactiveCommand<Unit, WalletUnlock> Confirm { get; }
    }
}
