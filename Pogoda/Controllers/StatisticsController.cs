
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pogoda.Data;

namespace Pogoda.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public StatisticsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var totalUsers = await _db.Users.CountAsync();
            var totalLocations = await _db.SavedLocations.CountAsync();

            var uniqueCities = await _db.SavedLocations
                .Select(location => location.Name)
                .Distinct()
                .CountAsync();

            var locations = await _db.SavedLocations
                .Join(
                    _db.Users,
                    location => location.UserId,
                    user => user.Id,
                    (location, user) => new
                    {
                        location.Name,
                        location.Address,
                        location.CreatedAt,
                        Username = user.Username
                    })
                .ToListAsync();

            var popularCities = locations
                .GroupBy(location => location.Name)
                .Select(group => new
                {
                    Name = group.Key,
                    Count = group.Count(),
                    Users = group
                        .Select(location => location.Username)
                        .Distinct()
                        .OrderBy(username => username)
                        .ToList()
                })
                .OrderByDescending(city => city.Count)
                .ThenBy(city => city.Name)
                .Take(10)
                .ToList();

            var recentLocations = locations
                .OrderByDescending(location => location.CreatedAt)
                .Take(10)
                .Select(location => new
                {
                    location.Name,
                    location.Address,
                    location.CreatedAt
                })
                .ToList();

            return Ok(new
            {
                totalUsers,
                totalLocations,
                uniqueCities,
                popularCities,
                recentLocations
            });
        }
    }
}