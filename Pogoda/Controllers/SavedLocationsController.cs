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
    public class SavedLocationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SavedLocationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        private int? GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(value, out var userId)
                ? userId
                : null;
        }

        [HttpGet]
        public async Task<IActionResult> GetLocations()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            var locations = await _db.SavedLocations
                .Where(location => location.UserId == userId.Value)
                .OrderByDescending(location => location.CreatedAt)
                .ToListAsync();

            return Ok(locations);
        }

        [HttpPost]
        public async Task<IActionResult> AddLocation(
            SavedLocationRequest request)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Name) ||
                request.Name.Trim().Length > 100 ||
                (request.Address?.Length ?? 0) > 255 ||
                request.Latitude < -90 || request.Latitude > 90 ||
                request.Longitude < -180 || request.Longitude > 180)
            {
                return BadRequest("Nieprawidłowe dane lokalizacji.");
            }

            var location = new SavedLocation
            {
                UserId = userId.Value,
                Name = request.Name.Trim(),
                Address = request.Address?.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            _db.SavedLocations.Add(location);
            await _db.SaveChangesAsync();

            return Ok(location);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            var location = await _db.SavedLocations
                .FirstOrDefaultAsync(location =>
                    location.Id == id &&
                    location.UserId == userId.Value);

            if (location == null)
                return NotFound();

            _db.SavedLocations.Remove(location);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }

    public class SavedLocationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}