using Microsoft.Data.Sqlite;
using System;
using System.Security.Cryptography;
using System.Text;

public class DatabaseService
{
    // Використовуйте крапку для поточного каталогу, якщо база поруч з .exe
    private string _connectionString = "Data Source=database.db"; 

    public DatabaseService()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
    }

    public bool UserExists(string email)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE email = @email";
            command.Parameters.AddWithValue("@email", email);

            // ВИПРАВЛЕНО: Безпечне отримання значення
            var result = command.ExecuteScalar();
            long count = result != null ? Convert.ToInt64(result) : 0;
            return count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DB CHECK ERROR: {ex.Message}");
            return false;
        }
    }

    public bool RegisterUser(string email, string password)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (username, email, password_hash) 
                VALUES (@name, @email, @hash)";
            
            command.Parameters.AddWithValue("@name", email.Contains('@') ? email.Split('@')[0] : email);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@hash", HashPassword(password));

            return command.ExecuteNonQuery() > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DB ERROR: {ex.Message}");
            return false;
        }
    }

    public bool ValidateUser(string email, string password)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            // ВИПРАВЛЕНО: замінено $email на @email для стабільності
            command.CommandText = "SELECT password_hash FROM Users WHERE email = @email";
            command.Parameters.AddWithValue("@email", email);

            var storedHash = command.ExecuteScalar()?.ToString();
            return storedHash != null && storedHash == HashPassword(password);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"VALIDATION ERROR: {ex.Message}");
            return false;
        }
    }

    private string HashPassword(string password)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}