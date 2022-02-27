using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.ViewModels.Fields;
using Avalonia.Controls;
using ReactiveUI;
using Solnet.Programs.Utilities;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Dialogs
{
    /// <summary>
    /// The view model of the dialog used to create a nonce account.
    /// </summary>
    public class CreateNonceAccountDialogViewModel : ViewModelBase
    {
        public CreateNonceAccountDialogViewModel()
        {
            this.WhenAnyValue(x => x.Authority.PublicKey)
                .Subscribe(x =>
                {
                    if (x != null)
                    {
                        IsInputValid = true;
                    }
                    else
                    {
                        IsInputValid = false;
                    }
                    this.RaisePropertyChanged(nameof(IsInputValid));
                });
        }

        /// <summary>
        /// The account being created.
        /// </summary>
        private Account _account = new();
        public Account Account
        {
            get => _account;
            set => this.RaiseAndSetIfChanged(ref _account, value);
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
        /// The address of the account.
        /// </summary>
        public string Address => Account.PublicKey;

        /// <summary>
        /// The authority of the account.
        /// </summary>
        private PublicKeyViewModel _authority = new();
        public PublicKeyViewModel Authority
        {
            get => _authority;
            set => this.RaiseAndSetIfChanged(ref _authority, value);
        }

        /// <summary>
        /// The rent for UI display.
        /// </summary>
        public decimal Rent { get => SolHelper.ConvertToSol(NativeRent); }

        /// <summary>
        /// The rent in lamports.
        /// </summary>
        public ulong NativeRent { get; init; }

        /// <summary>
        /// Whether all inputs are valid.
        /// </summary>
        private bool _isInputValid = false;
        public bool IsInputValid
        {
            get => _isInputValid;
            set => this.RaiseAndSetIfChanged(ref _isInputValid, value);
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
