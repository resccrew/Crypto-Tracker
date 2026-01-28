using Avalonia.Controls;
using Desktop_Crypto_Portfolio_Tracker.ViewModels;

namespace Desktop_Crypto_Portfolio_Tracker.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Ми створюємо ViewModel тут, щоб поєднати інтерфейс і логіку
        DataContext = new MainWindowViewModel();
    }
}