using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree; // Нужно для поиска родительского окна

namespace Desktop_Crypto_Portfolio_Tracker.Views
{
    public partial class PortfolioView : UserControl
    {
        public PortfolioView()
        {
            InitializeComponent();
        }

        private async void OnAddTransactionClick(object? sender, RoutedEventArgs e)
        {
            var dialog = new AddTransactionWindow();
            
            // Находим родительское окно, так как UserControl сам не является окном
            var topLevel = TopLevel.GetTopLevel(this) as Window;
            
            if (topLevel != null)
            {
                await dialog.ShowDialog(topLevel);
            }
        }
    }
}