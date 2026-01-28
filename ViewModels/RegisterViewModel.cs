using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Desktop_Crypto_Portfolio_Tracker.ViewModels;

public class RegisterViewModel : INotifyPropertyChanged
{
    private string _email = "";
    private string _password = "";
    private string _confirmPassword = "";
    private string _error = "";

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    public ICommand RegisterCommand { get; }
    public ICommand BackToLoginCommand { get; }

    public event Action? RegisterSucceeded;
    public event Action? BackToLoginRequested;

    public RegisterViewModel()
    {
        RegisterCommand = new RelayCommand(() =>
        {
            Error = "";

            if (string.IsNullOrWhiteSpace(Email))
            {
                Error = "Enter email.";
                return;
            }

            if (Password.Length < 8)
            {
                Error = "Password must have at least 8 symbols.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                Error = "Passwords don't match.";
                return;
            }

           
            RegisterSucceeded?.Invoke();
        });

        BackToLoginCommand = new RelayCommand(() =>
        {
            BackToLoginRequested?.Invoke();
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return;
        field = value;
        OnPropertyChanged(name);
    }
}
