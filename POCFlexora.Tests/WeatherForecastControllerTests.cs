using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using POCFlexora.Controllers;
using POCFlexora.Models;
using Xunit;

namespace POCFlexora.Tests
{
    public class WeatherForecastControllerTests
    {
        private readonly Mock<ILogger<WeatherForecastController>> _loggerMock;
        private readonly WeatherForecastController _controller;

        public WeatherForecastControllerTests()
        {
            WeatherForecastController.ResetForecasts();
            _loggerMock = new Mock<ILogger<WeatherForecastController>>();
            _controller = new WeatherForecastController(_loggerMock.Object);
        }

        // ── GetById ──────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(20)]
        public void GetById_ValidId_ReturnsOk(int id)
        {
            var result = _controller.GetById(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(21)]
        [InlineData(100)]
        public void GetById_InvalidId_ReturnsNotFound(int id)
        {
            var result = _controller.GetById(id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetById_ValidId_ReturnsForecastWithCorrectData()
        {
            var result = _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var forecast = Assert.IsType<WeatherForecast>(okResult.Value);
            Assert.NotNull(forecast.Summary);
        }

        // ── Create (POST) ───────────────────────────────────────────────────────────

        [Fact]
        public void Create_ValidRequest_ReturnsCreated()
        {
            var request = new CreateWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TemperatureC = 25,
                Summary = "Warm"
            };

            var result = _controller.Create(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var forecast = Assert.IsType<WeatherForecast>(createdResult.Value);
            Assert.Equal(request.TemperatureC, forecast.TemperatureC);
            Assert.Equal(request.Summary, forecast.Summary);
            Assert.Equal(request.Date, forecast.Date);
        }

        [Fact]
        public void Create_NullRequest_ReturnsBadRequest()
        {
            var result = _controller.Create(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Create_EmptySummary_ReturnsBadRequest()
        {
            var request = new CreateWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TemperatureC = 20,
                Summary = ""
            };

            var result = _controller.Create(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Create_WhitespaceSummary_ReturnsBadRequest()
        {
            var request = new CreateWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TemperatureC = 20,
                Summary = "   "
            };

            var result = _controller.Create(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Create_ValidRequest_IncreasesForecasts()
        {
            var countBefore = WeatherForecastController.Forecasts.Count;
            var request = new CreateWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TemperatureC = 15,
                Summary = "Cool"
            };

            _controller.Create(request);

            Assert.Equal(countBefore + 1, WeatherForecastController.Forecasts.Count);
        }

        [Fact]
        public void Create_ValidRequest_ReturnsCreatedAtActionWithCorrectRoute()
        {
            var request = new CreateWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                TemperatureC = 30,
                Summary = "Hot"
            };

            var result = _controller.Create(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(WeatherForecastController.GetById), createdResult.ActionName);
        }

        // ── Update (PUT) ────────────────────────────────────────────────────────────

        [Fact]
        public void Update_ValidRequest_ReturnsOk()
        {
            var request = new UpdateWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                TemperatureC = 20,
                Summary = "Warm"
            };

            var result = _controller.Update(1, request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void Update_ValidRequest_UpdatesForecastFields()
        {
            var newDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            var request = new UpdateWeatherForecastRequest
            {
                Date = newDate,
                TemperatureC = 35,
                Summary = "Scorching"
            };

            var result = _controller.Update(1, request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var forecast = Assert.IsType<WeatherForecast>(okResult.Value);
            Assert.Equal(newDate, forecast.Date);
            Assert.Equal(35, forecast.TemperatureC);
            Assert.Equal("Scorching", forecast.Summary);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(21)]
        [InlineData(100)]
        public void Update_InvalidId_ReturnsNotFound(int id)
        {
            var request = new UpdateWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = 10,
                Summary = "Cool"
            };

            var result = _controller.Update(id, request);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Update_NullRequest_ReturnsBadRequest()
        {
            var result = _controller.Update(1, null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Update_EmptySummary_ReturnsBadRequest()
        {
            var request = new UpdateWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = 10,
                Summary = ""
            };

            var result = _controller.Update(1, request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Update_WhitespaceSummary_ReturnsBadRequest()
        {
            var request = new UpdateWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = 10,
                Summary = "   "
            };

            var result = _controller.Update(1, request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(20)]
        public void Update_ValidId_UpdatesCorrectEntry(int id)
        {
            var request = new UpdateWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TemperatureC = 15,
                Summary = "Mild"
            };

            _controller.Update(id, request);

            var updated = WeatherForecastController.Forecasts[id - 1];
            Assert.Equal("Mild", updated.Summary);
            Assert.Equal(15, updated.TemperatureC);
        }

    }
}
