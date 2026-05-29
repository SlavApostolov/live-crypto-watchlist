using System.Text.Json;

namespace Live_Cryptocurrency_Watchlist.Services
{
    public class CryptoPriceService
    {
        private readonly HttpClient _httpClient;

        // Put your copied API key right here!
        private readonly string _apiKey = "";

        public CryptoPriceService(HttpClient client)
        {
            _httpClient = client;

            _httpClient.DefaultRequestHeaders.Add("x-cg-demo-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<decimal> GetPriceAsync(string coinId)
        {
            try
            {
                string safeCoinId = coinId.ToLower().Trim();

                string apiUrl = $"https://api.coingecko.com/api/v3/simple/price?ids={safeCoinId}&vs_currencies=usd";

                var response = await _httpClient.GetAsync(apiUrl);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new KeyNotFoundException($"Coin '{coinId}' does not exist.");

                if (!response.IsSuccessStatusCode) return 0.01m;

                var readingResponse = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(readingResponse);

                if (jsonDoc.RootElement.TryGetProperty(safeCoinId, out JsonElement coinData))
                {
                    return coinData.GetProperty("usd").GetDecimal();
                }
                return 0.01m;
            }
            catch (KeyNotFoundException) { throw; }
            catch { return 0.01m; }
        }

        public async Task<Dictionary<string, CoinData>> GetBulkPricesAsync(List<string> coinIds)
        {
            var results = new Dictionary<string, CoinData>();
            if (coinIds == null || coinIds.Count == 0) return results;

            try
            {
                string joinedIds = string.Join(",", coinIds).ToLower();
                string apiUrl = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&ids={joinedIds}";

                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return results;

                var readingResponse = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(readingResponse);

                if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var coin in jsonDoc.RootElement.EnumerateArray())
                    {
                        string id = coin.GetProperty("id").GetString();
                        decimal price = coin.GetProperty("current_price").GetDecimal();
                        string imageUrl = coin.GetProperty("image").GetString();

                        results[id] = new CoinData { Price = price, ImageUrl = imageUrl };
                    }
                }
            }
            catch { }

            return results;
        }

        public async Task<List<decimal[]>> GetHistoricalDataAsync(string coinId, int days = 7)
        {
            var prices = new List<decimal[]>();
            try
            {
                string apiUrl = $"https://api.coingecko.com/api/v3/coins/{coinId.ToLower().Trim()}/market_chart?vs_currency=usd&days={days}";

                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return prices;

                var readingResponse = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(readingResponse);

                if (jsonDoc.RootElement.TryGetProperty("prices", out JsonElement pricesElement) && pricesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in pricesElement.EnumerateArray())
                    {
                        prices.Add(new decimal[] { item[0].GetDecimal(), item[1].GetDecimal() });
                    }
                }
            }
            catch { }

            return prices;
        }
    }

    public class CoinData
    {
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }
}