using Avalonia.Controls;
using Avalonia.Interactivity;
using Desktop_Crypto_Portfolio_Tracker.Models;
using Desktop_Crypto_Portfolio_Tracker.ViewModels;
using System.Collections.Generic;

namespace Desktop_Crypto_Portfolio_Tracker.Views;

public partial class AddTransactionWindow : Window
{
    
    public AddTransactionWindow()
    {
        InitializeComponent();
    }

    
    public AddTransactionWindow(List<Coin> availableCoins) : this()
    {
        
        CoinComboBox.ItemsSource = availableCoins;
    }

    
    private void Coin_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CoinComboBox.SelectedItem is Coin selectedCoin)
        {
            
            PriceBox.Text = selectedCoin.Price.ToString();
        }
    }

    private void Add_Click(object? sender, RoutedEventArgs e)
    {
        
        if (CoinComboBox.SelectedItem is Coin selectedCoin &&
            decimal.TryParse(PriceBox.Text, out decimal price) && 
            decimal.TryParse(AmountBox.Text, out decimal amount))
        {
            var newItem = new PortfolioDisplayItem
            {
                CoinId = selectedCoin.Id,
                Name = selectedCoin.Name, 
                ImageUrl = selectedCoin.ImageUrl,
                Price = price,
                Amount = amount
            };

            Close(newItem);
        }
        else
        {
             
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}