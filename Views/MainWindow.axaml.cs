using Avalonia.Controls;
using Avalonia.Interactivity;
using Desktop_Crypto_Portfolio_Tracker.ViewModels;
using System.Linq;

namespace Desktop_Crypto_Portfolio_Tracker.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    // Додавання транзакції
    private async void OnAddTransactionClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var availableCoins = viewModel.MarketCoins.ToList();
            var dialog = new AddTransactionWindow(availableCoins);
            var result = await dialog.ShowDialog<PortfolioDisplayItem>(this);

            if (result != null)
            {
                viewModel.MyPortfolio.Add(result);
                viewModel.RecalculateBalance(); // Оновлюємо суму
            }
        }
    }

    // 👇 Логіка видалення
    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        // 1. Отримуємо кнопку, на яку натиснули
        var button = sender as Button;
        
        // 2. Дізнаємось, до якого запису (рядка) вона належить
        if (button?.DataContext is PortfolioDisplayItem itemToDelete)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                // 3. Видаляємо цей запис зі списку
                viewModel.MyPortfolio.Remove(itemToDelete);
                
                // 4. Перераховуємо баланс
                viewModel.RecalculateBalance();
            }
        }
    }
}