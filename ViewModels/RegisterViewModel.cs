using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;
using Desktop_Crypto_Portfolio_Tracker.Services;

namespace Desktop_Crypto_Portfolio_Tracker.ViewModels;

public class RegisterViewModel : INotifyPropertyChanged
{
    private string _email = "";
    private string _password = "";
    private string _confirmPassword = "";
    private string _error = "";

    private readonly DatabaseService _db = new DatabaseService();
    private readonly EmailService _emailService = new EmailService();

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    public ICommand RegisterCommand { get; }
    public ICommand BackToLoginCommand { get; }

    public event Action<long, string>? VerificationRequired;

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

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
            {
                Error = "Password must have at least 8 symbols.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                Error = "Passwords don't match.";
                return;
            }

            if (_db.UserExists(Email))
            {
                Error = "User with the same email already exists.";
                return;
            }


            var code = GenerateCode();
            var codeHash = Hash(code);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(10);

            try
            {

                var userId = _db.RegisterUserPendingVerification(Email, Password, codeHash, expiresAtUtc);
                if (userId <= 0)
                {
                    Error = "Failed to create a new user.";
                    return;
                }

                
                _emailService.SendVerificationCode(Email, code);

                
                VerificationRequired?.Invoke(userId, Email);
            }
            catch (Exception ex)
            {
                Error = "Registration failed.";
                Error = ex.Message; 
                System.Diagnostics.Debug.WriteLine(ex);
            }
        });

        BackToLoginCommand = new RelayCommand(() => BackToLoginRequested?.Invoke());
    }

    private static string GenerateCode()
    {
        
        var n = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return n.ToString("D6");
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
