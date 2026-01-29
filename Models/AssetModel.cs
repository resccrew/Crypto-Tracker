namespace Desktop_Crypto_Portfolio_Tracker.Models
{
    public class AssetModel
    {
        public int Rank { get; set; }           
        public string? Name { get; set; }        
        public string? Symbol { get; set; }      
        public decimal Price { get; set; }      
        public double Change24h { get; set; }   
        
        public decimal Amount { get; set; }     
        public decimal TotalValue => Price * Amount; 
    }
}