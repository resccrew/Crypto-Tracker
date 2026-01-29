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

            // Активуємо підтримку Foreign Keys
            using (var pragmaCmd = connection.CreateCommand())
            {
                pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
                await pragmaCmd.ExecuteNonQueryAsync();
            }

            using var transaction = connection.BeginTransaction();

            foreach (var coin in coins)
            {
                if (string.IsNullOrEmpty(coin.Id)) continue;
                
                // Викликаємо допоміжний метод для кожної монети
                await UpdateCoinDataAsync(connection, transaction, coin);
            }

            await transaction.CommitAsync();
            System.Diagnostics.Debug.WriteLine($"БАЗА ДАНИХ: Успішно оновлено {coins.Count} монет.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ПОМИЛКА ПРИ ЗБЕРЕЖЕННІ СПИСКУ МОНЕТ: {ex.Message}");
        }
    }

    private async Task UpdateCoinDataAsync(SqliteConnection connection, SqliteTransaction transaction, Coin coin)
    {
        double priceValue = (double)coin.Price;
        const int MaxHistoryRecords = 100; // Ліміт записів історії для кожної монети

        // 1. Оновлюємо поточну ціну (UPSERT)
        var updateCmd = connection.CreateCommand();
        updateCmd.Transaction = transaction;
        updateCmd.CommandText = @"
            INSERT INTO Coins (id, symbol, name, current_price)
            VALUES (@id, @symbol, @name, @price)
            ON CONFLICT(id) DO UPDATE SET 
                current_price = excluded.current_price,
                symbol = excluded.symbol,
                name = excluded.name;";

        updateCmd.Parameters.AddWithValue("@id", coin.Id);
        updateCmd.Parameters.AddWithValue("@symbol", coin.Symbol ?? "");
        updateCmd.Parameters.AddWithValue("@name", coin.Name ?? "");
        updateCmd.Parameters.AddWithValue("@price", priceValue);
        await updateCmd.ExecuteNonQueryAsync();

        // 2. Додаємо новий запис в історію
        var historyCmd = connection.CreateCommand();
        historyCmd.Transaction = transaction;
        historyCmd.CommandText = @"
            INSERT INTO Price_History (coin_id, price)
            VALUES (@coin_id, @price)";

        historyCmd.Parameters.AddWithValue("@coin_id", coin.Id);
        historyCmd.Parameters.AddWithValue("@price", priceValue);
        await historyCmd.ExecuteNonQueryAsync();

        // 3. ОЧИЩЕННЯ: Видаляємо найстаріші записи, якщо їх більше ніж MaxHistoryRecords
        // Цей запит видаляє всі записи для цієї монети, крім останніх N
        var cleanupCmd = connection.CreateCommand();
        cleanupCmd.Transaction = transaction;
        cleanupCmd.CommandText = @"
            DELETE FROM Price_History 
            WHERE coin_id = @coin_id 
            AND id NOT IN (
                SELECT id FROM Price_History 
                WHERE coin_id = @coin_id 
                ORDER BY timestamp DESC 
                LIMIT @limit
            );";

        cleanupCmd.Parameters.AddWithValue("@coin_id", coin.Id);
        cleanupCmd.Parameters.AddWithValue("@limit", MaxHistoryRecords);
        await cleanupCmd.ExecuteNonQueryAsync();
    }

}