using Microsoft.AspNetCore.Mvc;
using Pogoda.Clients;
using Pogoda.Models;
using Pogoda.Services;

namespace Pogoda.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IMeteoResolver _meteoResolver;
        private readonly GeocodingClient _geocodingClient;

        public WeatherForecastController(
            IMeteoResolver meteoResolver,
            GeocodingClient geocodingClient)
        {
            _meteoResolver = meteoResolver;
            _geocodingClient = geocodingClient;
        }

        [HttpGet]
        [Route("now")]
        public async Task<IActionResult> Get(
            double latitude,
            double longitude)
        {
            OpenMeteoResponse weather;

            try
            {
                weather = await _meteoResolver.GetDataAsync(
                    latitude,
                    longitude);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(weather);
        }

        [HttpGet]
        [Route("archive")]
        public async Task<IActionResult> GetArchive(
            double latitude,
            double longitude,
            string startDate,
            string endDate)
        {
            if (latitude < -90 || latitude > 90)
            {
                return BadRequest(
                    "Nieprawidłowa szerokość geograficzna.");
            }

            if (longitude < -180 || longitude > 180)
            {
                return BadRequest(
                    "Nieprawidłowa długość geograficzna.");
            }

            if (!DateTime.TryParse(startDate, out DateTime start))
            {
                return BadRequest(
                    "Nieprawidłowa data początkowa.");
            }

            if (!DateTime.TryParse(endDate, out DateTime end))
            {
                return BadRequest(
                    "Nieprawidłowa data końcowa.");
            }

            if (start > end)
            {
                return BadRequest(
                    "Data początkowa nie może być późniejsza niż data końcowa.");
            }

            if (start.Date > DateTime.Today)
            {
                return BadRequest(
                    "Data początkowa nie może być późniejsza niż obecna data.");
            }

            if (end.Date > DateTime.Today)
            {
                return BadRequest(
                    "Data końcowa nie może być późniejsza niż obecna data.");
            }

            OpenMeteoResponse weather;

            try
            {
                weather = await _meteoResolver.GetDataAsync(
                    latitude,
                    longitude,
                    startDate,
                    endDate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(weather);
        }

        [HttpGet]
        [Route("city")]
        public async Task<IActionResult> SearchCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("Podaj nazwę miasta.");
            }

            try
            {
                var location = await _geocodingClient.SearchCityAsync(city);

                if (location == null)
                {
                    return NotFound("Nie znaleziono miasta.");
                }

                return Ok(location);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("cities")]
        public async Task<IActionResult> SearchCities(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return Ok(new List<CityLocation>());
            }

            try
            {
                var cities = await _geocodingClient.SearchCitiesAsync(city);

                return Ok(cities);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}