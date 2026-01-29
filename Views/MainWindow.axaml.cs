using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Input; // Нужно для перетаскивания
using Desktop_Crypto_Portfolio_Tracker.ViewModels;
using System.Linq;

namespace Desktop_Crypto_Portfolio_Tracker.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel();
        DataContext = viewModel;
        _ = viewModel.InitializeAsync(1);
    }

    // ПЕРЕМЕЩЕНИЕ ОКНА
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    // Добавление транзакции
    private async void OnAddTransactionClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var availableCoins = viewModel.MarketCoins.ToList();
            var dialog = new AddTransactionWindow(availableCoins);
            
            // 👇 СТРОГО ИСПОЛЬЗУЕМ PortfolioDisplayItem (старый тип)
            var result = await dialog.ShowDialog<PortfolioDisplayItem>(this);

            if (result != null)
            {
                viewModel.MyPortfolio.Add(result);
                viewModel.RecalculateBalance();
            }
        }
    }

    // Удаление транзакции
    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        
        // 👇 СТРОГО ИСПОЛЬЗУЕМ PortfolioDisplayItem (старый тип)
        if (button?.DataContext is PortfolioDisplayItem itemToDelete)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.MyPortfolio.Remove(itemToDelete);
                viewModel.RecalculateBalance();
            }
        }
    }

    // Логика выхода (Logout)
    private void OnLogoutClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var loginWindow = new LoginWindow();
            desktop.MainWindow = loginWindow;
            loginWindow.Show();
        }

        Close();
    }
    
    // Заглушка для печати
    private void OnPrintClick(object? sender, RoutedEventArgs e)
    {
    }
}