using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Desktop_Crypto_Portfolio_Tracker.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private string _email = "";
    private string _password = "";
    private string _error = "";
    private readonly DatabaseService _db = new DatabaseService();

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string Error { get => _error; set => SetProperty(ref _error, value); }

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

            if (_db.ValidateUser(Email, Password))
            {
                LoginSucceeded?.Invoke();
            }
            if (!_db.UserExists(Email))
{
    Error = "User not found in DB";
    return;
}

if (!_db.IsEmailVerified(Email))
{
    Error = "Email not verified";
    return;
}

Error = "Password mismatch";
            
            
            
        });

        GoToRegisterCommand = new RelayCommand(() => RegisterRequested?.Invoke());
    }

    // Реалізація інтерфейсу для оновлення UI
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return;
        field = value;
        OnPropertyChanged(name);
    }
}