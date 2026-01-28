using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using Desktop_Crypto_Portfolio_Tracker.Models;
using Desktop_Crypto_Portfolio_Tracker.Services;

namespace Desktop_Crypto_Portfolio_Tracker.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly CoinGeckoService _coinService;
    private decimal _totalBalance;

    public ObservableCollection<Coin> MarketCoins { get; } = new();
    public ObservableCollection<PortfolioDisplayItem> MyPortfolio { get; } = new();

    // Властивість для загального балансу
    public decimal TotalBalance
    {
        get => _totalBalance;
        set
        {
            _totalBalance = value;
            OnPropertyChanged(); // Повідомляємо інтерфейс про зміну
        }
    }

    public MainWindowViewModel()
    {
        _coinService = new CoinGeckoService();
        _ = LoadMarketDataAsync();
        
        // Початковий баланс 0
        RecalculateBalance();
    }

    private async Task LoadMarketDataAsync()
    {
        var coins = await _coinService.GetTopCoinsAsync();
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MarketCoins.Clear();
            foreach (var coin in coins)
            {
                coin.Symbol = coin.Symbol?.ToUpper();
                MarketCoins.Add(coin);
            }
        });
    }

    // Метод перерахунку балансу
    public void RecalculateBalance()
    {
        TotalBalance = MyPortfolio.Sum(item => item.TotalValue);
    }

    // Стандартна реалізація повідомлення про зміни (MVVM)
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class PortfolioDisplayItem
{
    public string? Name { get; set; } 
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalValue => Price * Amount;
    public string? ImageUrl { get; set; }
}