using System.Text.Json;

namespace Live_Cryptocurrency_Watchlist.Services
{
    public class CryptoPriceService
    {
        private readonly HttpClient _httpClient;

        public CryptoPriceService(HttpClient client)
        {
            _httpClient = client;
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<decimal> GetPriceAsync(string coinId)
        {
            try
            {
                string safeCoinId = coinId.ToLower().Trim();
                string apiUrl = $"https://api.coincap.io/v2/assets/{safeCoinId}";

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"Coin '{coinId}' does not exist.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    return 0.01m;
                }

                var readingResponse = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(readingResponse);

                string priceString = jsonDoc.RootElement.GetProperty("data").GetProperty("priceUsd").GetString();
                if (decimal.TryParse(priceString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price))
                {
                    return price;
                }
                return 0.01m;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch
            {
                return 0.01m;
            }
        }

        public async Task<Dictionary<string, CoinData>> GetBulkPricesAsync(List<string> coinIds)
        {
            var results = new Dictionary<string, CoinData>();
            if (coinIds == null || coinIds.Count == 0) return results;

            try
            {
                string joinedIds = string.Join(",", coinIds).ToLower();
                string apiUrl = $"https://api.coincap.io/v2/assets?ids={joinedIds}";

                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return results;

                var readingResponse = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(readingResponse);

                if (jsonDoc.RootElement.TryGetProperty("data", out JsonElement dataArray) && dataArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var coin in dataArray.EnumerateArray())
                    {
                        string id = coin.GetProperty("id").GetString();
                        string symbol = coin.GetProperty("symbol").GetString().ToLower();
                        string priceString = coin.GetProperty("priceUsd").GetString();

                        // CoinCap hosts icons using the symbol
                        string imageUrl = $"https://assets.coincap.io/assets/icons/{symbol}@2x.png";

                        if (decimal.TryParse(priceString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price))
                        {
                            results[id] = new CoinData { Price = price, ImageUrl = imageUrl };
                        }
                    }
                }
            }
            catch
            {
            }

            return results;
        }

        public async Task<List<decimal[]>> GetHistoricalDataAsync(string coinId, int days = 7)
        {
            var prices = new List<decimal[]>();
            try
            {
                long end = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long start = DateTimeOffset.UtcNow.AddDays(-days).ToUnixTimeMilliseconds();

                string apiUrl = $"https://api.coincap.io/v2/assets/{coinId.ToLower().Trim()}/history?interval=h6&start={start}&end={end}";

                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return prices;

                var readingResponse = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(readingResponse);

                if (jsonDoc.RootElement.TryGetProperty("data", out JsonElement dataArray) && dataArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in dataArray.EnumerateArray())
                    {
                        long time = item.GetProperty("time").GetInt64();
                        string priceStr = item.GetProperty("priceUsd").GetString();

                        if (decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal priceVal))
                        {
                            prices.Add(new decimal[] { time, priceVal });
                        }
                    }
                }
            }
            catch
            {

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