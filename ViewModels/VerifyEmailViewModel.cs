using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;

namespace Desktop_Crypto_Portfolio_Tracker.ViewModels;

public class VerifyEmailViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _db = new DatabaseService();
    private string _code = "";
    private string _error = "";
    private string _infoText;

    public long UserId { get; }
    public string Email { get; }

    public string InfoText
    {
        get => _infoText;
        set => SetProperty(ref _infoText, value);
    }

    public string Code { get => _code; set => SetProperty(ref _code, value); }
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    public ICommand VerifyCommand { get; }
    public ICommand BackCommand { get; }

    public event Action? Verified;
    public event Action? BackRequested;

    public VerifyEmailViewModel(long userId, string email)
    {
        UserId = userId;
        Email = email;
        _infoText = $"We sent a code to {email}. Enter it below.";

        VerifyCommand = new RelayCommand(() =>
        {
            Error = "";

            var trimmed = (Code ?? "").Trim();
            if (trimmed.Length == 0)
            {
                Error = "Enter the code.";
                return;
            }

            var codeHash = Hash(trimmed);

            if (_db.VerifyEmailCode(UserId, codeHash))
                Verified?.Invoke();
            else
                Error = "Invalid or expired code.";
        });

        BackCommand = new RelayCommand(() => BackRequested?.Invoke());
    }

    private static string Hash(string input)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
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
