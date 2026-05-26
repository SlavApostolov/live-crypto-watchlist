namespace Live_Cryptocurrency_Watchlist.Models
{
    public class SavedCoin
    {
        public int Id { get; set; }

        public string? CoinId { get; set; }

        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public User? User { get; set; }
    }
}
