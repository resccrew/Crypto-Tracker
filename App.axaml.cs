using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Desktop_Crypto_Portfolio_Tracker.ViewModels;
using Desktop_Crypto_Portfolio_Tracker.Views; // Цей рядок обов'язковий!

namespace Desktop_Crypto_Portfolio_Tracker;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var loginWindow = new LoginWindow();

            loginWindow.Closed += (_, __) =>
            {
                // якщо користувач закрив логін — закриваємо додаток
                if (desktop.MainWindow == null || !desktop.MainWindow.IsVisible)
                desktop.Shutdown();
            };

            // Показуємо логін першим
            loginWindow.Show();

            // ВАЖЛИВО: не виставляємо MainWindow відразу
            // desktop.MainWindow буде створено після успішного логіну
        }

        base.OnFrameworkInitializationCompleted();
    }
}