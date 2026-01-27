using Avalonia.Controls;
using Desktop_Crypto_Portfolio_Tracker.ViewModels;

namespace Desktop_Crypto_Portfolio_Tracker.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(); // Это связывает код и дизайн
    }
}