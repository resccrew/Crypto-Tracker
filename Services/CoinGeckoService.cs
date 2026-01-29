using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Desktop_Crypto_Portfolio_Tracker.Models;

namespace Desktop_Crypto_Portfolio_Tracker.Services;

public class CoinGeckoService
{
    private readonly HttpClient _httpClient;

    public CoinGeckoService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AvaloniaCryptoTracker/1.0");
    }

    public async Task<List<Coin>> GetTopCoinsAsync()
    {
        try
        {
            string url = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=50&page=1&sparkline=false";
            
            var coins = await _httpClient.GetFromJsonAsync<List<Coin>>(url);
            if (coins != null)
            {
                var db = new DatabaseService();
                
                await db.SaveCoinsToDbAsync(coins);
                
                return coins;
            }
            return new List<Coin>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Помилка отримання даних: {ex.Message}");
            return new List<Coin>();
        }
    }
}
