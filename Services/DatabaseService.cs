using Microsoft.Data.Sqlite;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Desktop_Crypto_Portfolio_Tracker.Models;
using System.Xml;
using System.Collections.Generic;
using System.IO;



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
        command.CommandText = @"
            SELECT password_hash, email_verified
            FROM Users
            WHERE email = @email
            LIMIT 1;";
        command.Parameters.AddWithValue("@email", email);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return false;

        var storedHash = reader.GetString(0);
        var verified = reader.GetInt32(1) == 1;

        if (!verified) return false;

        return storedHash == HashPassword(password);
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"VALIDATION ERROR: {ex}");
        return false;
    }
}


    private string HashPassword(string password)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public long RegisterUserPendingVerification(string email, string password, string codeHash, DateTime expiresAtUtc)
    {
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO Users (username, email, password_hash, email_verified, email_verification_code_hash, email_verification_expires_at)
        VALUES (@name, @email, @hash, 0, @codeHash, @expiresAt);
        SELECT last_insert_rowid();";

    command.Parameters.AddWithValue("@name", email.Contains('@') ? email.Split('@')[0] : email);
    command.Parameters.AddWithValue("@email", email);
    command.Parameters.AddWithValue("@hash", HashPassword(password));
    command.Parameters.AddWithValue("@codeHash", codeHash);
    command.Parameters.AddWithValue("@expiresAt", expiresAtUtc.ToString("O"));

    var result = command.ExecuteScalar();
    return result != null ? Convert.ToInt64(result) : 0;
    }

    public bool VerifyEmailCode(long userId, string codeHash)
    {
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    // 1) дістаємо очікуваний хеш і дедлайн
    var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        SELECT email_verification_code_hash, email_verification_expires_at, email_verified
        FROM Users
        WHERE id = @id
        LIMIT 1;";
    cmd.Parameters.AddWithValue("@id", userId);

    using var reader = cmd.ExecuteReader();
    if (!reader.Read()) return false;

    var storedHash = reader.IsDBNull(0) ? null : reader.GetString(0);
    var expiresStr = reader.IsDBNull(1) ? null : reader.GetString(1);
    var verified = !reader.IsDBNull(2) && reader.GetInt32(2) == 1;

    if (verified) return true;
    if (storedHash == null || expiresStr == null) return false;

    if (!DateTime.TryParse(expiresStr, out var expiresAt))
        return false;

    if (DateTime.UtcNow > expiresAt.ToUniversalTime())
        return false;

    if (storedHash != codeHash)
        return false;

    // 2) підтверджуємо email
    var upd = connection.CreateCommand();
    upd.CommandText = @"
        UPDATE Users
        SET email_verified = 1,
            email_verification_code_hash = NULL,
            email_verification_expires_at = NULL
        WHERE id = @id;";
    upd.Parameters.AddWithValue("@id", userId);

    return upd.ExecuteNonQuery() > 0;
    }

    public bool IsEmailVerified(string email)
    {
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT email_verified FROM Users WHERE email = @email LIMIT 1;";
    cmd.Parameters.AddWithValue("@email", email);

    var result = cmd.ExecuteScalar();
    return result != null && Convert.ToInt32(result) == 1;
    }
    public async Task SaveCoinsToDbAsync(List<Coin> coins)
    {
        if (coins == null || coins.Count == 0) return;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            var command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = @"
                INSERT OR REPLACE INTO Coins (id, symbol, name, current_price)
                VALUES (@id, @symbol, @name, @price)";

            var pId = command.Parameters.Add("@id", SqliteType.Text);
            var pSymbol = command.Parameters.Add("@symbol", SqliteType.Text);
            var pName = command.Parameters.Add("@name", SqliteType.Text);
            var pPrice = command.Parameters.Add("@price", SqliteType.Real);

            foreach (var coin in coins)
            {
                pId.Value = coin.Id ?? (object)DBNull.Value;
                pSymbol.Value = coin.Symbol ?? "";
                pName.Value = coin.Name ?? "";
                pPrice.Value = (double)coin.Price;

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            System.Diagnostics.Debug.WriteLine($"БАЗА ДАНИХ: Асинхронно оновлено {coins.Count} монет.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ПОМИЛКА АСИНХРОННОГО ЗБЕРЕЖЕННЯ: {ex.Message}");
        }
    }

}