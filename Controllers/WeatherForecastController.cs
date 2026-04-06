using Microsoft.AspNetCore.Mvc;

namespace POCFlexora.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private static readonly IReadOnlyList<WeatherForecast> Forecasts =
            Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = (index * 7) - 10,
                Summary = new[]
                {
                    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
                    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
                }[index % 10]
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
    }
}
