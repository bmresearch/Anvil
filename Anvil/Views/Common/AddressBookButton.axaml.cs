using Anvil.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anvil.Views.Common
{
    public partial class AddressBookButton : UserControl
    {
        public static readonly StyledProperty<string> SelectedAddressProperty =
            AvaloniaProperty.Register<AddressBookButton, string>(nameof(SelectedAddress));

        public string SelectedAddress
        {
            get { return GetValue(SelectedAddressProperty); }
            set { SetValue(SelectedAddressProperty, value); }
        }

        public ListBox AddressBookItems => this.FindControl<ListBox>("addressBookList");

        public Button Button => this.FindControl<Button>("flyOutButton");

        public AddressBookButton()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            AddressBookItems.SelectionChanged += AddressBookItems_SelectionChanged;
        }

        private void AddressBookItems_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var newItem = e.AddedItems[0];

                if (newItem is AddressBookItem w)
                {
                    SelectedAddress = w.Address;
                }

                AddressBookItems.SelectedItem = null;
                Button.Flyout.Hide();
            }
            e.Handled = true;
        }
    }
}
