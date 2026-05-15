using Live_Cryptocurrency_Watchlist.Data;
using Live_Cryptocurrency_Watchlist.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Live_Cryptocurrency_Watchlist.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatchlistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WatchlistController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public IActionResult GetUser()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }
    }
}
