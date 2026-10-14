using Pogoda.Services;
using System.Globalization;
using System.Text.Json;

namespace Pogoda.Clients
{
    public class OpenMeteoClient : IMeteoClient
    {
        private readonly HttpClient _client;

        public OpenMeteoClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<OpenMeteoResponse> GetDataAsync(
            double latitude,
            double longitude,
            string? startDate = null,
            string? endDate = null)
        {
            var url = "https://api.open-meteo.com/v1/forecast" +
                      $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
                      $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
                      "&current=temperature_2m,wind_speed_10m,relative_humidity_2m,cloud_cover" +
                      "&timezone=auto";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<OpenMeteoResponse>(data)!;
        }
    }

}