using Anvil.ViewModels.Wallet;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Anvil.Views.Wallet
{
    public partial class ImportWalletView : ReactiveUserControl<ImportWalletViewModel>
    {
        public ImportWalletView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
