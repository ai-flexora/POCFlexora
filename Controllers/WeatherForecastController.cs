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
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching", "VeryHighHeat"
        };

        internal static List<WeatherForecast> Forecasts =
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

        internal static void ResetForecasts()
        {
            Forecasts = Enumerable.Range(1, 20).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = (index * 3) - 10,
                Summary = Summaries[index % 10]
            }).ToList();
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

[HttpPost(Name = "CreateWeatherForecast")]
        public IActionResult Create([FromBody] CreateWeatherForecastRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required.");

                if (string.IsNullOrWhiteSpace(request.Summary))
                    return BadRequest("Summary is required.");

                var forecast = new WeatherForecast
                {
                    Date = request.Date,
                    TemperatureC = request.TemperatureC,
                    Summary = request.Summary
                };

                Forecasts.Add(forecast);

                return CreatedAtAction(nameof(GetById), new { id = Forecasts.Count }, forecast);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating weather forecast");
                return StatusCode(500);
            }
        }

        [HttpPut("{id}", Name = "UpdateWeatherForecast")]
        public IActionResult Update(int id, [FromBody] UpdateWeatherForecastRequest request)
        {
            try
            {
                if (id < 1 || id > Forecasts.Count)
                    return NotFound();

                if (request == null)
                    return BadRequest("Request body is required.");

                if (string.IsNullOrWhiteSpace(request.Summary))
                    return BadRequest("Summary is required.");

                var forecast = Forecasts[id - 1];
                forecast.Date = request.Date;
                forecast.TemperatureC = request.TemperatureC;
                forecast.Summary = request.Summary;

                return Ok(forecast);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating forecast for id {Id}", id);
                return StatusCode(500);
            }
        }

    }
}
