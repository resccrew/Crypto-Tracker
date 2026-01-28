using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using Desktop_Crypto_Portfolio_Tracker.Models;
using Desktop_Crypto_Portfolio_Tracker.Services;

namespace Desktop_Crypto_Portfolio_Tracker.ViewModels;

public class MainWindowViewModel 
{
    private readonly CoinGeckoService _coinService;

    public ObservableCollection<Coin> MarketCoins { get; } = new();
    public ObservableCollection<PortfolioDisplayItem> MyPortfolio { get; } = new();

    public MainWindowViewModel()
    {
        _coinService = new CoinGeckoService();
        _ = LoadMarketDataAsync();
        LoadDummyPortfolio();
    }

    private async Task LoadMarketDataAsync()
    {
        var coins = await _coinService.GetTopCoinsAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MarketCoins.Clear();
            foreach (var coin in coins)
            {
                coin.Symbol = coin.Symbol.ToUpper();
                MarketCoins.Add(coin);
            }
        });
    }

    private void LoadDummyPortfolio()
    {
        MyPortfolio.Add(new PortfolioDisplayItem { Name = "Bitcoin", Price = 64000.50m, Amount = 0.5m });
        MyPortfolio.Add(new PortfolioDisplayItem { Name = "Ethereum", Price = 3450.00m, Amount = 10.0m });
        MyPortfolio.Add(new PortfolioDisplayItem { Name = "Solana", Price = 145.20m, Amount = 50.0m });
    }
}

public class PortfolioDisplayItem
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalValue => Price * Amount;
}