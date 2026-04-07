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

        private readonly List<WeatherForecast> _forecasts =
            Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = (index * 7) - 10,
                Summary = new[]
                {
                    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
                    "Warm", "Balmy", "Hot", "Sweltering", "Scorching","VeryHighHeat","shouldlearntolerate"
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
            return _forecasts;
        }

        [HttpGet("{id}", Name = "GetWeatherForecastById")]
        public IActionResult GetById(int id)
        {
            try
            {
                if (id < 1 || id > _forecasts.Count)
                    return NotFound();

                return Ok(_forecasts[id - 1]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forecast for id {Id}", id);
                return StatusCode(500);
            }
        }

        [HttpDelete("{id}", Name = "DeleteWeatherForecastById")]
        public IActionResult Delete(int id)
        {
            try
            {
                if (id < 1 || id > _forecasts.Count)
                    return NotFound();

                _forecasts.RemoveAt(id - 1);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting forecast for id {Id}", id);
                return StatusCode(500);
            }
        }
    }
}
