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
            _loggerMock = new Mock<ILogger<WeatherForecastController>>();
            _controller = new WeatherForecastController(_loggerMock.Object);
        }

        // ── GetById ──────────────────────────────────────────────────────────

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

        // ── GetByIdPaged ─────────────────────────────────────────────────────

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
    }
}
