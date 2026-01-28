using System;
using System.Windows.Input;

namespace Desktop_Crypto_Portfolio_Tracker.ViewModels;

public class LoginViewModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Error { get; set; } = "";

    public ICommand LoginCommand { get; }

    public ICommand GoToRegisterCommand { get; }

    public event Action? LoginSucceeded;

    public event Action? RegisterRequested;

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(() =>
        {
            Error = "";

            if (string.IsNullOrWhiteSpace(Email))
            {
                Error = "Enter email.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                Error = "Enter password.";
                return;
            }

            
            LoginSucceeded?.Invoke();
        });

        GoToRegisterCommand = new RelayCommand(() =>
        {
            RegisterRequested?.Invoke();
        });
    }
}
