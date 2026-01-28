using Avalonia.Controls;
using Avalonia.Interactivity;
using Desktop_Crypto_Portfolio_Tracker.ViewModels;
using System.Linq; // Потрібно для .ToList()

namespace Desktop_Crypto_Portfolio_Tracker.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private async void OnAddTransactionClick(object? sender, RoutedEventArgs e)
    {
        // Отримуємо доступ до ViewModel, щоб взяти з неї список монет
        if (DataContext is MainWindowViewModel viewModel)
        {
            // Беремо список монет, який вже завантажений у першій вкладці
            var availableCoins = viewModel.MarketCoins.ToList();

            // Передаємо цей список у конструктор вікна
            var dialog = new AddTransactionWindow(availableCoins);

            var result = await dialog.ShowDialog<PortfolioDisplayItem>(this);

            if (result != null)
            {
                viewModel.MyPortfolio.Add(result);
            }
        }
    }
}