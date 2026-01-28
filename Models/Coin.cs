using System.Text.Json.Serialization;

namespace Desktop_Crypto_Portfolio_Tracker.Models;

public class Coin
{
    [JsonPropertyName("market_cap_rank")]
    public int Rank { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("current_price")]
    public decimal Price { get; set; }

    [JsonPropertyName("price_change_percentage_24h")]
    public double Change24h { get; set; }

    public string UpperSymbol => Symbol?.ToUpper() ?? "";
}