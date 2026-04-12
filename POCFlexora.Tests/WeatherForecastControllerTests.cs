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

        // ── GetByIdPaged ────────────────────────────────────────────────────────────

        [Fact]
        public void GetByIdPaged_ValidIdAndPage_ReturnsOkWithPagedResult()
        {
            var result = _controller.GetByIdPaged(1, pageNumber: 1, pageSize: 5);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var paged = Assert.IsType<PagedResult<WeatherForecast>>(okResult.Value);
            Assert.Equal(1, paged.PageNumber);
            Assert.Equal(5, paged.PageSize);
            Assert.Equal(5, paged.Items.Count());
        }

        [Fact]
        public void GetByIdPaged_ValidIdSecondPage_ReturnsCorrectSlice()
        {
            var result = _controller.GetByIdPaged(1, pageNumber: 2, pageSize: 5);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var paged = Assert.IsType<PagedResult<WeatherForecast>>(okResult.Value);
            Assert.Equal(2, paged.PageNumber);
            Assert.Equal(5, paged.Items.Count());
        }

        [Fact]
        public void GetByIdPaged_ValidIdLastPage_ReturnsRemainingItems()
        {
            // Start from id=16, 5 records remain (16-20), pageSize=10 → only 5 returned
            var result = _controller.GetByIdPaged(16, pageNumber: 1, pageSize: 10);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var paged = Assert.IsType<PagedResult<WeatherForecast>>(okResult.Value);
            Assert.Equal(5, paged.TotalRecords);
            Assert.Equal(5, paged.Items.Count());
        }

        [Fact]
        public void GetByIdPaged_TotalPagesCalculatedCorrectly()
        {
            // id=1 → 20 records total, pageSize=5 → 4 pages
            var result = _controller.GetByIdPaged(1, pageNumber: 1, pageSize: 5);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var paged = Assert.IsType<PagedResult<WeatherForecast>>(okResult.Value);
            Assert.Equal(4, paged.TotalPages);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(21)]
        public void GetByIdPaged_InvalidId_ReturnsNotFound(int id)
        {
            var result = _controller.GetByIdPaged(id, pageNumber: 1, pageSize: 5);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1, 0, 5)]
        [InlineData(1, 1, 0)]
        [InlineData(1, -1, 5)]
        [InlineData(1, 1, -1)]
        public void GetByIdPaged_InvalidPaginationParams_ReturnsBadRequest(int id, int pageNumber, int pageSize)
        {
            var result = _controller.GetByIdPaged(id, pageNumber, pageSize);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void GetByIdPaged_PageBeyondAvailableData_ReturnsNotFound()
        {
            // id=1, 20 records, pageSize=5 → 4 pages; page 5 has no data
            var result = _controller.GetByIdPaged(1, pageNumber: 5, pageSize: 5);

            Assert.IsType<NotFoundResult>(result);
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

        // ── Patch ───────────────────────────────────────────────────────────────────

        [Fact]
        public void Patch_ValidRequestAllFields_ReturnsOk()
        {
            var request = new PatchWeatherForecastRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                TemperatureC = 20,
                Summary = "Warm"
            };

            var result = _controller.Patch(1, request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void Patch_OnlySummary_UpdatesSummaryOnly()
        {
            var originalDate = WeatherForecastController.Forecasts[0].Date;
            var originalTemp = WeatherForecastController.Forecasts[0].TemperatureC;
            var request = new PatchWeatherForecastRequest { Summary = "Breezy" };

            _controller.Patch(1, request);

            var updated = WeatherForecastController.Forecasts[0];
            Assert.Equal("Breezy", updated.Summary);
            Assert.Equal(originalDate, updated.Date);
            Assert.Equal(originalTemp, updated.TemperatureC);
        }

        [Fact]
        public void Patch_OnlyTemperature_UpdatesTemperatureOnly()
        {
            var originalDate = WeatherForecastController.Forecasts[0].Date;
            var originalSummary = WeatherForecastController.Forecasts[0].Summary;
            var request = new PatchWeatherForecastRequest { TemperatureC = 42 };

            _controller.Patch(1, request);

            var updated = WeatherForecastController.Forecasts[0];
            Assert.Equal(42, updated.TemperatureC);
            Assert.Equal(originalDate, updated.Date);
            Assert.Equal(originalSummary, updated.Summary);
        }

        [Fact]
        public void Patch_OnlyDate_UpdatesDateOnly()
        {
            var newDate = DateOnly.FromDateTime(DateTime.Now.AddDays(99));
            var originalTemp = WeatherForecastController.Forecasts[0].TemperatureC;
            var originalSummary = WeatherForecastController.Forecasts[0].Summary;
            var request = new PatchWeatherForecastRequest { Date = newDate };

            _controller.Patch(1, request);

            var updated = WeatherForecastController.Forecasts[0];
            Assert.Equal(newDate, updated.Date);
            Assert.Equal(originalTemp, updated.TemperatureC);
            Assert.Equal(originalSummary, updated.Summary);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(21)]
        [InlineData(100)]
        public void Patch_InvalidId_ReturnsNotFound(int id)
        {
            var request = new PatchWeatherForecastRequest { Summary = "Cloudy" };

            var result = _controller.Patch(id, request);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Patch_NullRequest_ReturnsBadRequest()
        {
            var result = _controller.Patch(1, null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Patch_WhitespaceSummary_ReturnsBadRequest()
        {
            var request = new PatchWeatherForecastRequest { Summary = "   " };

            var result = _controller.Patch(1, request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Patch_EmptySummary_ReturnsBadRequest()
        {
            var request = new PatchWeatherForecastRequest { Summary = "" };

            var result = _controller.Patch(1, request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Patch_EmptyRequest_ReturnsOkWithUnchangedForecast()
        {
            var original = WeatherForecastController.Forecasts[0];
            var originalDate = original.Date;
            var originalTemp = original.TemperatureC;
            var originalSummary = original.Summary;
            var request = new PatchWeatherForecastRequest();

            _controller.Patch(1, request);

            var updated = WeatherForecastController.Forecasts[0];
            Assert.Equal(originalDate, updated.Date);
            Assert.Equal(originalTemp, updated.TemperatureC);
            Assert.Equal(originalSummary, updated.Summary);
        }
    }
}
