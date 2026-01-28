using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Desktop_Crypto_Portfolio_Tracker.ViewModels;

namespace Desktop_Crypto_Portfolio_Tracker.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        ShowLogin();
    }

    private void ShowLogin()
    {
        var loginVm = new LoginViewModel();
        loginVm.LoginSucceeded += OnLoginSucceeded;
        loginVm.RegisterRequested += ShowRegister;

        DataContext = loginVm;
    }

    private void ShowRegister()
    {
        var registerVm = new RegisterViewModel();
        registerVm.RegisterSucceeded += ShowLogin;
        registerVm.BackToLoginRequested += ShowLogin;

        DataContext = registerVm;
    }

    private void OnLoginSucceeded()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.Show();
        }

        Close();
    }
}
