using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.ViewModels.Fields;
using Avalonia.Controls;
using ReactiveUI;
using Solnet.Extensions;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;
using System;
using System.Collections.Generic;

namespace Anvil.ViewModels.Dialogs
{
    /// <summary>
    /// The view model used to send tokens.
    /// </summary>
    public class SendTokenDialogViewModel : ViewModelBase
    {
        public SendTokenDialogViewModel()
        {
            this.WhenAnyValue(x => x.Destination.PublicKey)
                .Subscribe(x =>
                {
                    if (x != null)
                    {
                        _isDestinationValid = true;
                    }
                    else
                    {
                        _isDestinationValid = false;
                    }
                    this.RaisePropertyChanged("IsInputValid");
                });
        }

        /// <summary>
        /// The currently selected token.
        /// </summary>
        public TokenWalletBalance SelectedToken { get; set; }

        /// <summary>
        /// The available tokens.
        /// </summary>
        public List<TokenWalletBalance> Tokens { get; set; }

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
        /// The maximum amount to transfer.
        /// </summary>
        private decimal MaxAmount { get => SelectedToken.QuantityDecimal; }

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

        /// <summary>
        /// Whether the destination is valid.
        /// </summary>
        private bool _isDestinationValid;

        /// <summary>
        /// Whether all inputs are valid.
        /// </summary>
        public bool IsInputValid => _isAmountValid && _isDestinationValid;

        /// <summary>
        /// Validate the token amount.
        /// </summary>
        /// <exception cref="Avalonia.Data.DataValidationException">Exception thrown if the amount is invalid.</exception>
        private void ValidateAmountDecimal()
        {
            _isAmountValid = Amount > 0m;
            this.RaisePropertyChanged("IsInputValid");
            if (!_isAmountValid) throw new Avalonia.Data.DataValidationException("Invalid amount!");

            _isAmountValid = _isAmountValid && Amount <= SelectedToken.QuantityDecimal;
            this.RaisePropertyChanged("IsInputValid");
            if (!_isAmountValid) throw new Avalonia.Data.DataValidationException("Insufficient balance!");
        }

        /// <summary>
        /// Sets the amount to the maximum value possible.
        /// </summary>
        public void Max()
        {
            Amount = MaxAmount;
        }

        /// <summary>
        /// Whether the action is confirmed or not.
        /// </summary>
        public bool Confirmed { get; private set; }

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
