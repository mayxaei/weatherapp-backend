using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pogoda.Data;
using System.Security.Claims;

namespace Pogoda.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherChartsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public WeatherChartsController(ApplicationDbContext db)
        {
            _db = db;
        }

        private int? GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        [HttpGet]
        public async Task<IActionResult> GetCharts()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            var charts = await _db.WeatherCharts
                .Where(chart => chart.UserId == userId.Value)
                .OrderByDescending(chart => chart.CreatedAt)
                .ToListAsync();

            return Ok(charts);
        }

        [HttpPost]
        public async Task<IActionResult> AddChart(WeatherChartRequest request)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Name) ||
                request.Name.Trim().Length > 100 ||
                string.IsNullOrWhiteSpace(request.LocationName) ||
                request.LocationName.Trim().Length > 100 ||
                request.Latitude < -90 || request.Latitude > 90 ||
                request.Longitude < -180 || request.Longitude > 180 ||
                request.StartDate == default ||
                request.EndDate == default ||
                request.StartDate.Date > request.EndDate.Date)
            {
                return BadRequest("Nieprawidłowe dane wykresu.");
            }

            var name = request.Name.Trim();

            var nameExists = await _db.WeatherCharts.AnyAsync(chart =>
                chart.UserId == userId.Value &&
                chart.Name.ToLower() == name.ToLower());

            if (nameExists)
                return Conflict("Wykres o tej nazwie już istnieje. Wybierz inną nazwę.");

            var chart = new WeatherChart
            {
                UserId = userId.Value,
                Name = name,
                LocationName = request.LocationName.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate.Date,
                CreatedAt = DateTime.UtcNow
            };

            _db.WeatherCharts.Add(chart);
            await _db.SaveChangesAsync();

            return Ok(chart);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateChart(int id, WeatherChartRequest request)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Name) ||
                request.Name.Trim().Length > 100 ||
                string.IsNullOrWhiteSpace(request.LocationName) ||
                request.LocationName.Trim().Length > 100 ||
                request.Latitude < -90 || request.Latitude > 90 ||
                request.Longitude < -180 || request.Longitude > 180 ||
                request.StartDate == default ||
                request.EndDate == default ||
                request.StartDate.Date > request.EndDate.Date)
            {
                return BadRequest("Nieprawidłowe dane wykresu.");
            }

            var chart = await _db.WeatherCharts.FirstOrDefaultAsync(chart =>
                chart.Id == id && chart.UserId == userId.Value);

            if (chart == null)
                return NotFound();

            var name = request.Name.Trim();

            var nameExists = await _db.WeatherCharts.AnyAsync(other =>
                other.UserId == userId.Value &&
                other.Id != id &&
                other.Name.ToLower() == name.ToLower());

            if (nameExists)
                return Conflict("Wykres o tej nazwie już istnieje. Wybierz inną nazwę.");

            chart.Name = name;
            chart.LocationName = request.LocationName.Trim();
            chart.Latitude = request.Latitude;
            chart.Longitude = request.Longitude;
            chart.StartDate = request.StartDate.Date;
            chart.EndDate = request.EndDate.Date;

            await _db.SaveChangesAsync();

            return Ok(chart);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteChart(int id)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            var chart = await _db.WeatherCharts
                .FirstOrDefaultAsync(chart =>
                    chart.Id == id && chart.UserId == userId.Value);

            if (chart == null)
                return NotFound();

            _db.WeatherCharts.Remove(chart);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }

    public class WeatherChartRequest
    {
        public string Name { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}