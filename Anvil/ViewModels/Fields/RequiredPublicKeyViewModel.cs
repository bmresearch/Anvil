using Anvil.Services;
using ReactiveUI;
using System.Collections.Generic;

namespace Anvil.ViewModels.Fields
{
    public class RequiredPublicKeyViewModel : PublicKeyViewModel
    {
        public RequiredPublicKeyViewModel(bool isRequired, AddressBookService addressBookService)
        {
            _isRequired = isRequired;
            AddressBookService = addressBookService;
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

        private bool _isRequired;
        public bool IsRequired
        {
            get => _isRequired;
            set => this.RaiseAndSetIfChanged(ref _isRequired, value);
        }
    }
}
