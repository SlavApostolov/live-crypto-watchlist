using System.Text.Json;

namespace Live_Cryptocurrency_Watchlist.Services
{
    public class CryptoPriceService
    {
        private readonly HttpClient _httpClient;
        public CryptoPriceService(HttpClient client) 
        {
            _httpClient = client;

            // Tell CoinGecko who is making the request to avoid the 403 block
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Live_Cryptocurrency_Watchlist_App");
        }

        public async Task<decimal> GetPriceAsync(string coinId)
        {
            string apiUrl = $"https://api.coingecko.com/api/v3/simple/price?ids={coinId}&vs_currencies=usd";
            var response = await _httpClient.GetAsync(apiUrl);

            var readingResponse = await response.Content.ReadAsStringAsync();

            var priceData = JsonDocument.Parse(readingResponse)
                            .RootElement.GetProperty(coinId)
                            .GetProperty("usd")
                            .GetDecimal();

            return priceData;
        }       
    }
}
