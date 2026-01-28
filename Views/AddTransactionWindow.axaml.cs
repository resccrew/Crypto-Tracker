using Avalonia.Controls;
using Avalonia.Interactivity;
using Desktop_Crypto_Portfolio_Tracker.Models;
using Desktop_Crypto_Portfolio_Tracker.ViewModels;
using System.Collections.Generic;

namespace Desktop_Crypto_Portfolio_Tracker.Views;

public partial class AddTransactionWindow : Window
{
    // Конструктор за замовчуванням (потрібен для прев'ю XAML)
    public AddTransactionWindow()
    {
        InitializeComponent();
    }

    // Новий конструктор, який приймає список монет
    public AddTransactionWindow(List<Coin> availableCoins) : this()
    {
        // Заповнюємо випадаючий список монетами, які ми передали
        CoinComboBox.ItemsSource = availableCoins;
    }

    // Коли користувач обирає монету зі списку
    private void Coin_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CoinComboBox.SelectedItem is Coin selectedCoin)
        {
            // Автоматично підставляємо актуальну ціну
            PriceBox.Text = selectedCoin.Price.ToString();
        }
    }

    private void Add_Click(object? sender, RoutedEventArgs e)
    {
        // Перевіряємо, чи вибрана монета і чи введені числа
        if (CoinComboBox.SelectedItem is Coin selectedCoin &&
            decimal.TryParse(PriceBox.Text, out decimal price) && 
            decimal.TryParse(AmountBox.Text, out decimal amount))
        {
            var newItem = new PortfolioDisplayItem
            {
                Name = selectedCoin.Name, // Беремо назву з обраної монети
                ImageUrl = selectedCoin.ImageUrl,
                Price = price,
                Amount = amount
            };

            Close(newItem);
        }
        else
        {
             // Якщо валідація не пройшла
             // Можна додати червону рамку або повідомлення, але поки просто не закриваємо
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}