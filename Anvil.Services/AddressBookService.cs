using Anvil.Services.Wallets.SubWallets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services
{
    public class AddressBookItem
    {
        public string Address { get; set; }

        public string Alias { get; set; }
        public string ShortenedAddress
        {
            get => Address[..6] + "..." + Address[^6..];
        }
    }

    /// <summary>
    /// Implements an address book.
    /// </summary>
    public class AddressBookService
    {
        /// <summary>
        /// The watch-only account store.
        /// </summary>
        private IWatchOnlyAccountStore _accountStore;

        /// <summary>
        /// The wallet service.
        /// </summary>
        private IWalletService _walletService;

        /// <summary>
        /// The multisig account store.
        /// </summary>
        private IMultiSignatureAccountMappingStore _multiSignatureAccountMappingStore;

        /// <summary>
        /// Initialize the address book service with the given services.
        /// </summary>
        /// <param name="walletService">The wallet service.</param>
        /// <param name="multiSignatureAccountMappingStore">The multisig account store.</param>
        /// <param name="watchOnlyAccountStore">The watch-only account store.</param>
        public AddressBookService(IWalletService walletService,
            IMultiSignatureAccountMappingStore multiSignatureAccountMappingStore,
            IWatchOnlyAccountStore watchOnlyAccountStore)
        {
            _accountStore = watchOnlyAccountStore;
            _walletService = walletService;
            _multiSignatureAccountMappingStore = multiSignatureAccountMappingStore;
        }

        public List<AddressBookItem> GetItems()
        {
            var items = new List<AddressBookItem>();

            items.AddRange(_walletService.Wallets.Select(x => new AddressBookItem
            {
                Address = x.Address,
                Alias = x.Alias
            }));
            items.AddRange(_accountStore.WatchOnlyAccounts.Select(x => new AddressBookItem
            {
                Address = x.Address,
                Alias = x.Alias
            }));
            items.AddRange(_multiSignatureAccountMappingStore.MultiSignatureAccountMappings.Select(x => new AddressBookItem
            {
                Address = x.Address,
                Alias = x.Alias
            }));

            return items;
        }
    }
}
