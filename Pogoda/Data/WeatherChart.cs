namespace Pogoda.Data
{
    public class WeatherChart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}