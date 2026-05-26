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

            var jsonDoc = JsonDocument.Parse(readingResponse);
            string safeCoinId = coinId.ToLower().Trim();

            if (jsonDoc.RootElement.TryGetProperty(safeCoinId, out JsonElement coinData))
                return coinData.GetProperty("usd").GetDecimal();
            //var priceData = JsonDocument.Parse(readingResponse)
            //                .RootElement.GetProperty(coinId)
            //                .GetProperty("usd")
            //                .GetDecimal();

            return 0m;
        }

        public async Task<Dictionary<string, CoinData>> GetBulkPricesAsync(List<string> coinIds)
        {
            var results = new Dictionary<string, CoinData>();
            if (coinIds == null || coinIds.Count == 0) return results;

            string joinedIds = string.Join(",", coinIds).ToLower();

            string apiUrl = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&ids={joinedIds}";

            var response = await _httpClient.GetAsync(apiUrl);
            var readingResponse = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(readingResponse);

            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var coin in jsonDoc.RootElement.EnumerateArray())
                {
                    string id = coin.GetProperty("id").GetString();
                    decimal price = coin.GetProperty("current_price").GetDecimal();
                    string imageUrl = coin.GetProperty("image").GetString(); // Grab the image!

                    results[id] = new CoinData { Price = price, ImageUrl = imageUrl };

                    //string safeCoin = coin.ToLower().Trim();
                    //if (jsonDoc.RootElement.TryGetProperty(safeCoin, out JsonElement coinData))
                    //{
                    //    results[safeCoin] = coinData.GetProperty("usd").GetDecimal();
                    //}
                    //else
                    //{
                    //    results[safeCoin] = 0m;
                    //}
                }
            }

            return results;
        }

        public async Task<List<decimal[]>> GetHistoricalDataAsync(string coinId, int days = 7)
        {
            string apiUrl = $"https://api.coingecko.com/api/v3/coins/{coinId.ToLower().Trim()}/market_chart?vs_currency=usd&days={days}";

            var response = await _httpClient.GetAsync(apiUrl);
            var readingResponse = await response.Content.ReadAsStringAsync();

            using var jsonDoc = JsonDocument.Parse(readingResponse);
            var prices = new List<decimal[]>();

            if (jsonDoc.RootElement.TryGetProperty("prices", out JsonElement pricesElement) && pricesElement.ValueKind == JsonValueKind.Array)
            {
                // CoinGecko returns an array of arrays: [ [timestamp, price], [timestamp, price] ]
                foreach (var item in pricesElement.EnumerateArray())
                {
                    // Add the timestamp and the price to our list
                    prices.Add(new decimal[] { item[0].GetDecimal(), item[1].GetDecimal() });
                }
            }

            return prices;
        }
    }

    public class CoinData
    {
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }
}
