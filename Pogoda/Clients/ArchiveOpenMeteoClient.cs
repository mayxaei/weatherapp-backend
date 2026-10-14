using System.Globalization;
using System.Text.Json;
using Pogoda.Services;

namespace Pogoda.Clients
{
    public class ArchiveOpenMeteoClient : IMeteoClient
    {
        private readonly HttpClient _client;

        public ArchiveOpenMeteoClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<OpenMeteoResponse> GetDataAsync(
            double latitude,
            double longitude,
            string? startDate = null,
            string? endDate = null)
        {
            var url = "https://archive-api.open-meteo.com/v1/archive" +
                      $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
                      $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
                      $"&start_date={startDate}" +
                      $"&end_date={endDate}" +
                      "&daily=temperature_2m_mean" +
                      "&timezone=auto";

            var response = await _client.GetAsync(url);

            var data = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Open-Meteo error: {data}");
            }

            return JsonSerializer.Deserialize<OpenMeteoResponse>(data)!;
        }
    }

}