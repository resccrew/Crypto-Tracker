using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Input; 
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

   
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    
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
                viewModel.RecalculateBalance();
            }
        }
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        
        if (button?.DataContext is PortfolioDisplayItem itemToDelete)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.MyPortfolio.Remove(itemToDelete);
                viewModel.RecalculateBalance();
            }
        }
    }

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
    
    private void OnPrintClick(object? sender, RoutedEventArgs e)
    {
    }
}