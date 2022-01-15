using Anvil.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Anvil.Views
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
