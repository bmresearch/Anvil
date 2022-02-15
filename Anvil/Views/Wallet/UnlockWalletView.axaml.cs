using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anvil.Views.Wallet
{
    public partial class UnlockWalletView : UserControl
    {
        public UnlockWalletView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
