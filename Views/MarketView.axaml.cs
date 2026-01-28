using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Desktop_Crypto_Portfolio_Tracker.Views
{
    public partial class MarketView : UserControl
    {
        public MarketView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}