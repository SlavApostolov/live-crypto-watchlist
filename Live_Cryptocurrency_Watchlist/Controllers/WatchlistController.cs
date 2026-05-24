using Live_Cryptocurrency_Watchlist.Services;
using Live_Cryptocurrency_Watchlist.Models;
using Live_Cryptocurrency_Watchlist.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Live_Cryptocurrency_Watchlist.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatchlistController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CryptoPriceService _cryptoPriceService;

        public WatchlistController(AppDbContext context, CryptoPriceService cryptoPriceService)
        {
            _context = context;
            _cryptoPriceService = cryptoPriceService;
        }

        [HttpGet("users")]
        public IActionResult GetUser()
        {
            var users = _context.Users.Include(u => u.SavedCoins).ToList();
            return Ok(users);
        }

        [HttpPost("users")]
        public IActionResult CreateUser([FromBody] User newUser)
        {
            if (newUser == null || string.IsNullOrEmpty(newUser.Username))
            {
                return BadRequest("Invalid user data.");
            }

            _context.Users.Add(newUser);
            _context.SaveChanges();
            return Ok(newUser);
        }

        [HttpPost("coins")]
        public IActionResult AddCoins([FromBody] SavedCoin newCoin)
        {
            if (newCoin == null || string.IsNullOrEmpty(newCoin.CoinId))
            {
                return BadRequest("Invalid coin data.");
            }

            bool exist = _context.Users.Any(u => u.UserId == newCoin.UserId);

            if (!exist)
            {
                return NotFound("User not found.");
            }

            _context.SavedCoins.Add(newCoin);
            _context.SaveChanges();
            return Ok(newCoin);
        }

        [HttpGet("{userId}/portfolio")]
        public async Task<IActionResult> GetPortfolio(int userId)
        {
            var user = _context.Users.Include(u => u.SavedCoins).FirstOrDefault(u => u.UserId == userId);

            if(user == null)
            {
                return BadRequest("Invalid user ID.");
            }

            var portfolioData = new List<object>();

            foreach (var savedCoin in user.SavedCoins)
            {
                decimal currentPrice = await _cryptoPriceService.GetPriceAsync(savedCoin.CoinId);
                portfolioData.Add(new
                {
                    CoinId = savedCoin.CoinId,
                    CurrentPrice = currentPrice
                });
            }

            return Ok(new 
            {
                Username = user.Username,
                Portfolio = portfolioData
            });
        }
    }
}
