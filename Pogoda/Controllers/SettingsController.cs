
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Pogoda.Data;

namespace Pogoda.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = userId,
                    TemperatureUnit = "C",
                    WindSpeedUnit = "kmh",
                    Theme = "system",
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserSettings.Add(settings);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    _context.Entry(settings).State = EntityState.Detached;

                    settings = await _context.UserSettings
                        .FirstOrDefaultAsync(s => s.UserId == userId);

                    if (settings == null)
                        throw;
                }
            }

            return Ok(new
            {
                settings.TemperatureUnit,
                settings.WindSpeedUnit,
                settings.Theme
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings(
            [FromBody] UpdateSettingsRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (request == null)
                return BadRequest("Brak danych ustawień.");

            if (request.TemperatureUnit != "C" &&
                request.TemperatureUnit != "F")
                return BadRequest("Nieprawidłowa jednostka temperatury.");

            if (request.WindSpeedUnit != "kmh" &&
                request.WindSpeedUnit != "ms")
                return BadRequest("Nieprawidłowa jednostka prędkości wiatru.");

            if (request.Theme != "dark" &&
                request.Theme != "light" &&
                request.Theme != "system")
                return BadRequest("Nieprawidłowy motyw.");

            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                return Conflict(
                    "Nie znaleziono ustawień użytkownika. Odśwież stronę i spróbuj ponownie."
                );
            }

            settings.TemperatureUnit = request.TemperatureUnit;
            settings.WindSpeedUnit = request.WindSpeedUnit;
            settings.Theme = request.Theme;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ustawienia zostały zapisane.",
                settings.TemperatureUnit,
                settings.WindSpeedUnit,
                settings.Theme
            });
        }
    }

    public class UpdateSettingsRequest
    {
        public string TemperatureUnit { get; set; } = "C";
        public string WindSpeedUnit { get; set; } = "kmh";
        public string Theme { get; set; } = "system";
    }
}