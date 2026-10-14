namespace Pogoda.Data
{
    public class SavedLocation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}