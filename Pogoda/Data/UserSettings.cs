
namespace Pogoda.Data
{
    public class UserSettings
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TemperatureUnit { get; set; } = "C";
        public string WindSpeedUnit { get; set; } = "kmh";
        public string Theme { get; set; } = "system";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
