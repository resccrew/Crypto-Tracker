using System;
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

    private readonly DatabaseService _dbService = new DatabaseService();

    public ObservableCollection<Coin> MarketCoins { get; } = new();
    public ObservableCollection<PortfolioDisplayItem> MyPortfolio { get; } = new();

    // --- БАЛАНС ---
    public decimal TotalBalance
    {
        get => _totalBalance;
        set
        {
            _totalBalance = value;
            OnPropertyChanged();
        }
    }

    // --- ЛОГІКА ПОРІВНЯННЯ (COMPARATOR) ---
    private Coin? _selectedCoinA;
    private Coin? _selectedCoinB;

    public Coin? SelectedCoinA
    {
        get => _selectedCoinA;
        set
        {
            _selectedCoinA = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HypotheticalPrice));
            OnPropertyChanged(nameof(PriceMultiplier));
        }
    }

    public Coin? SelectedCoinB
    {
        get => _selectedCoinB;
        set
        {
            _selectedCoinB = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HypotheticalPrice));
            OnPropertyChanged(nameof(PriceMultiplier));
        }
    }

    // Обрахунок нової ціни: PriceA * (CapB / CapA)
    public decimal HypotheticalPrice
    {
        get
        {
            if (SelectedCoinA == null || SelectedCoinB == null || SelectedCoinA.MarketCap == 0) return 0;
            return SelectedCoinA.Price * (SelectedCoinB.MarketCap / SelectedCoinA.MarketCap);
        }
    }

    // Обрахунок множника (CapB / CapA)
    public double PriceMultiplier
    {
        get
        {
            if (SelectedCoinA == null || SelectedCoinB == null || SelectedCoinA.MarketCap == 0) return 0;
            return (double)(SelectedCoinB.MarketCap / SelectedCoinA.MarketCap);
        }
    }

    // --- КОНСТРУКТОР ---
    public MainWindowViewModel()
    {
        _coinService = new CoinGeckoService();
        _ = LoadMarketDataAsync();
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

    public void RecalculateBalance()
    {
        TotalBalance = MyPortfolio.Sum(item => item.TotalValue);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task LoadPortfolioAsync(long userId)
    {
        var db = new DatabaseService();
        var items = await db.GetUserPortfolioAsync(userId);
        
        MyPortfolio.Clear();
        foreach (var item in items)
        {
            MyPortfolio.Add(item);
        }
        RecalculateBalance();
    }

    public async Task InitializeAsync(long userId)
    {
        Console.WriteLine("--> Початок ініціалізації портфеля...");
        await LoadMarketDataAsync();

        var transactions = await _dbService.GetUserPortfolioAsync(userId);
        Console.WriteLine($"--> База повернула записів: {transactions.Count}");

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MyPortfolio.Clear();
            foreach (var item in transactions)
            {
                Console.WriteLine($"--> Додаю в UI: {item.Name} ({item.Amount} шт.)");
                MyPortfolio.Add(item);
            }
            RecalculateBalance();
        });
    }
}

// --- КЛАС ЕЛЕМЕНТА ПОРТФОЛІО ---
public class PortfolioDisplayItem : INotifyPropertyChanged
{
    public long DbId { get; set; }      // Внутрішній ID транзакції в SQLite
    public string? CoinId { get; set; }
    public string? Symbol { get; set; }
    private string? _name;
    private decimal _price;
    private decimal _amount;
    private string? _imageUrl;

    public string? Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public decimal Price
    {
        get => _price;
        set 
        { 
            _price = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(TotalValue)); 
        }
    }

    public decimal Amount
    {
        get => _amount;
        set 
        { 
            _amount = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(TotalValue)); 
        }
    }

    public decimal TotalValue => Price * Amount;

    public string? ImageUrl
    {
        get => _imageUrl;
        set { _imageUrl = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}