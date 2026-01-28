using Microsoft.Data.Sqlite;
using System;
using System.Security.Cryptography;
using System.Text;

public class DatabaseService
{
    private string _connectionString = "Data Source=D:\\projects\\dropstab\\Crypto-Tracker\\database.db";

    public DatabaseService()
    {
        // Ініціалізація БД (створення таблиць, якщо їх немає)
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        // Тут можна виконати ваш SQL скрипт CREATE TABLE...
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

            // ExecuteScalar повертає кількість знайдених рядків
            long count = (long)command.ExecuteScalar();
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
            // Використовуємо @ для параметрів
            command.CommandText = @"
                INSERT INTO Users (username, email, password_hash) 
                VALUES (@name, @email, @hash)";
            
            command.Parameters.AddWithValue("@name", email.Split('@')[0]);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@hash", HashPassword(password));

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            // Дивіться сюди у вікні Output (Вивід) під час натискання кнопки
            System.Diagnostics.Debug.WriteLine($"DB ERROR: {ex.Message}");
            return false;
        }
    }

    public bool ValidateUser(string email, string password)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT password_hash FROM Users WHERE email = $email";
        command.Parameters.AddWithValue("$email", email);

        var storedHash = command.ExecuteScalar()?.ToString();
        return storedHash != null && storedHash == HashPassword(password);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}