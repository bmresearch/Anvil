using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anvil.Views.Crafter
{
    public partial class TransactionCraftView : UserControl
    {
        public TransactionCraftView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
