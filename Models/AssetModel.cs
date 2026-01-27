namespace Desktop_Crypto_Portfolio_Tracker.Models
{
    public class AssetModel
    {
        public int Rank { get; set; }           // Для вкладки Рынок (№1, №2...)
        public string Name { get; set; }        // Bitcoin
        public string Symbol { get; set; }      // BTC
        public decimal Price { get; set; }      // $95,000
        public double Change24h { get; set; }   // +2.5%
        
        // Поля только для Портфеля (в Рынке они будут 0)
        public decimal Amount { get; set; }     // 0.5 BTC
        public decimal TotalValue => Price * Amount; // $47,500
    }
}