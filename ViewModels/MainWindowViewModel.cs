using System.Collections.ObjectModel;
using Desktop_Crypto_Portfolio_Tracker.Models;

namespace Desktop_Crypto_Portfolio_Tracker.ViewModels
{
    public class MainWindowViewModel
    {
        // Список 1: Все монеты рынка (Вкладка "Market")
        public ObservableCollection<AssetModel> MarketCoins { get; set; }

        // Список 2: Личные монеты пользователя (Вкладка "Portfolio")
        public ObservableCollection<AssetModel> MyPortfolio { get; set; }

        public MainWindowViewModel()
        {
            // 1. Заполняем "Рынок" (Имитация данных с API)
            MarketCoins = new ObservableCollection<AssetModel>
            {
                new AssetModel { Rank=1, Name="Bitcoin", Symbol="BTC", Price=96000, Change24h=1.2 },
                new AssetModel { Rank=2, Name="Ethereum", Symbol="ETH", Price=2700, Change24h=-0.5 },
                new AssetModel { Rank=3, Name="Solana", Symbol="SOL", Price=145, Change24h=5.1 },
                new AssetModel { Rank=4, Name="BNB", Symbol="BNB", Price=600, Change24h=0.2 },
                new AssetModel { Rank=5, Name="Ripple", Symbol="XRP", Price=0.6m, Change24h=-1.0 }
            };

            // 2. Заполняем "Портфель" (Имитация данных из Базы Данных)
            MyPortfolio = new ObservableCollection<AssetModel>
            {
                new AssetModel { Name="Bitcoin", Symbol="BTC", Price=96000, Amount=0.5m, Change24h=1.2 },
                new AssetModel { Name="Solana", Symbol="SOL", Price=145, Amount=100, Change24h=5.1 }
            };
        }
    }
}