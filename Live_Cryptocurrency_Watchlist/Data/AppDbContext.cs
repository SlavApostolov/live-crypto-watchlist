using Microsoft.EntityFrameworkCore;
using Live_Cryptocurrency_Watchlist.Models;

namespace Live_Cryptocurrency_Watchlist.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options){ }
        public DbSet<User> Users { get; set; }
        public DbSet<SavedCoin> SavedCoins { get; set; }
    }
}
