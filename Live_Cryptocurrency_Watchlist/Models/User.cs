namespace Live_Cryptocurrency_Watchlist.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string? Username { get; set; }

        public string Password { get; set; }

        public List<SavedCoin>? SavedCoins { get; set; }
    }
}
