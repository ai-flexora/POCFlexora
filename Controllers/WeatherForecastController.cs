using Microsoft.AspNetCore.Mvc;
using POCFlexora.Models;

namespace POCFlexora.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching","VeryHighHeat"
        };

        private static readonly IReadOnlyList<WeatherForecast> Forecasts =
            Enumerable.Range(1, 20).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = (index * 3) - 10,
                Summary = Summaries[index % 10]
            }).ToList();

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Forecasts;
        }

        [HttpGet("{id}", Name = "GetWeatherForecastById")]
        public IActionResult GetById(int id)
        {
            try
            {
                if (id < 1 || id > Forecasts.Count)
                    return NotFound();

                return Ok(Forecasts[id - 1]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forecast for id {Id}", id);
                return StatusCode(500);
            }
        }

        [HttpGet("{id}/paged", Name = "GetWeatherForecastByIdPaged")]
        public IActionResult GetByIdPaged(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                if (id < 1 || id > Forecasts.Count)
                    return NotFound();

                if (pageNumber < 1 || pageSize < 1)
                    return BadRequest("pageNumber and pageSize must be greater than 0.");

                var anchor = Forecasts[id - 1];
                var anchorIndex = id - 1;

                var remaining = Forecasts.Skip(anchorIndex).ToList();
                var totalRecords = remaining.Count;
                var items = remaining
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (!items.Any())
                    return NotFound();

                var result = new PagedResult<WeatherForecast>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged forecasts for id {Id}", id);
                return StatusCode(500);
            }
        }
    }
}
