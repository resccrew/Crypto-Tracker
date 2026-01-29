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

    // --- КОЛЕКЦІЇ ---
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
}

// --- КЛАС ЕЛЕМЕНТА ПОРТФОЛІО ---
public class PortfolioDisplayItem : INotifyPropertyChanged
{
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