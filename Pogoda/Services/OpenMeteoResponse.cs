using System.Text.Json.Serialization;

namespace Pogoda.Services
{
    public class OpenMeteoResponse
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("current")]
        public Current? Current { get; set; }

        [JsonPropertyName("daily")]
        public Daily? Daily { get; set; }
    }

    public class Current
    {
        [JsonPropertyName("temperature_2m")]
        public double Temperature_2m { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double Wind_speed_10m { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public double Relative_humidity_2m { get; set; }

        [JsonPropertyName("cloud_cover")]
        public double Cloud_cover { get; set; }
    }

    public class Daily
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        [JsonPropertyName("temperature_2m_mean")]
        public List<double> Temperature_2m_mean { get; set; } = new();
    }
}