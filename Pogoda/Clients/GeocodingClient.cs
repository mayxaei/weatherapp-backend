using System.Text.Json;
using Pogoda.Models;

namespace Pogoda.Clients
{
    public class GeocodingClient
    {
        private readonly HttpClient _httpClient;

        public GeocodingClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CityLocation>> SearchCitiesAsync(string city)
        {
            var url =
                $"https://geocoding-api.open-meteo.com/v1/search" +
                $"?name={Uri.EscapeDataString(city)}" +
                $"&count=10" +
                $"&language=pl" +
                $"&format=json";

            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            var cities = new List<CityLocation>();

            if (!document.RootElement.TryGetProperty("results", out var results))
            {
                return cities;
            }

            foreach (var result in results.EnumerateArray())
            {
                cities.Add(new CityLocation
                {
                    Name = result.GetProperty("name").GetString() ?? "",
                    Country = result.TryGetProperty("country", out var country)
                        ? country.GetString() ?? ""
                        : "",
                    Latitude = result.GetProperty("latitude").GetDouble(),
                    Longitude = result.GetProperty("longitude").GetDouble()
                });
            }

            return cities
                .OrderBy(c => c.Name)
                .Take(5)
                .ToList();
        }

        public async Task<CityLocation?> SearchCityAsync(string city)
        {
            var cities = await SearchCitiesAsync(city);

            return cities.FirstOrDefault();
        }
    }
}