using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.ViewModels.Fields;
using Avalonia.Controls;
using ReactiveUI;
using Solnet.Programs.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Dialogs
{
    public class SendSolanaDialogViewModel : ViewModelBase
    {
        public SendSolanaDialogViewModel()
        {
            this.WhenAnyValue(x => x.Destination.PublicKey)
                .Subscribe(x =>
                {
                    if (x != null)
                    {
                        IsDestinationValid = true;
                    }
                    else
                    {
                        IsDestinationValid = false;
                    }
                    this.RaisePropertyChanged(nameof(IsInputValid));
                });
        }

        /// <summary>
        /// The currently selected item in the address book.
        /// </summary>
        private AddressBookItem _selectedAddressBookItem;
        public AddressBookItem SelectedAddressBookItem
        {
            get => _selectedAddressBookItem;
            set => this.RaiseAndSetIfChanged(ref _selectedAddressBookItem, value);
        }

        /// <summary>
        /// The address book items.
        /// </summary>
        public List<AddressBookItem> AddressBookItems
        {
            get => AddressBookService.GetItems();
        }

        /// <summary>
        /// The address book service.
        /// </summary>
        public AddressBookService AddressBookService { get; init; }

        /// <summary>
        /// The balance.
        /// </summary>
        public decimal Balance { get => SolHelper.ConvertToSol(NativeBalance); }

        /// <summary>
        /// The native balance in lamports.
        /// </summary>
        public ulong NativeBalance { get; init; }

        /// <summary>
        /// The native balance in lamports.
        /// </summary>
        private ulong MaxNativeAmount { get => NativeBalance - 5000; }

        /// <summary>
        /// The maximum amount after accounting for transaction fee.
        /// </summary>
        private decimal MaxAmount { get => SolHelper.ConvertToSol(MaxNativeAmount); }

        /// <summary>
        /// The amount to transfer.
        /// </summary>
        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                this.RaiseAndSetIfChanged(ref _amount, value);
                ValidateAmountDecimal();
            }
        }

        /// <summary>
        /// The destination of the transfer.
        /// </summary>
        private PublicKeyViewModel _destination = new();
        public PublicKeyViewModel Destination
        {
            get => _destination;
            set => this.RaiseAndSetIfChanged(ref _destination, value);
        }

        /// <summary>
        /// Whether the amount is valid.
        /// </summary>
        private bool _isAmountValid;

        private bool _isDestinationValid;
        public bool IsDestinationValid
        {
            get => _isDestinationValid;
            set => this.RaiseAndSetIfChanged(ref _isDestinationValid, value);
        }

        /// <summary>
        /// Whether all inputs are valid.
        /// </summary>
        public bool IsInputValid => _isAmountValid && IsDestinationValid;

        /// <summary>
        /// Validate the token amount.
        /// </summary>
        /// <exception cref="Avalonia.Data.DataValidationException">Exception thrown if the amount is invalid or the balance is insufficient.</exception>
        private void ValidateAmountDecimal()
        {
            _isAmountValid = Amount > 0m;
            this.RaisePropertyChanged(nameof(IsInputValid));
            if (!_isAmountValid) throw new Avalonia.Data.DataValidationException("Invalid amount!");

            _isAmountValid = _isAmountValid && Amount <= Balance;
            this.RaisePropertyChanged(nameof(IsInputValid));
            if (!_isAmountValid) throw new Avalonia.Data.DataValidationException("Insufficient balance!");

            _isAmountValid = _isAmountValid && Amount <= MaxAmount;
            this.RaisePropertyChanged(nameof(IsInputValid));
            if (!_isAmountValid) throw new Avalonia.Data.DataValidationException("Insufficient balance after accounting for transaction fee!");
        }

        /// <summary>
        /// Whether the action is confirmed or not.
        /// </summary>
        public bool Confirmed { get; private set; }

        /// <summary>
        /// Sets the amount to the maximum value possible.
        /// </summary>
        public void Max()
        {
            Amount = MaxAmount;
        }

        /// <summary>
        /// Cancels the action.
        /// </summary>
        /// <param name="window">The current window.</param>
        public void Cancel(Window window)
        {
            window.Close();
        }

        /// <summary>
        /// Confirms the action.
        /// </summary>
        /// <param name="window">The current window.</param>
        public void Confirm(Window window)
        {
            Confirmed = true;
            window.Close();
        }
    }
}
