using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using POCFlexora.Controllers;
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

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void GetById_ValidId_ReturnsOk(int id)
        {
            var result = _controller.GetById(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(6)]
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

        [Fact]
        public void Delete_ValidId_ReturnsNoContent()
        {
            var result = _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(100)]
        public void Delete_InvalidId_ReturnsNotFound(int id)
        {
            var result = _controller.Delete(id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_ValidId_RemovesItemFromList()
        {
            var controller = new WeatherForecastController(_loggerMock.Object);
            var beforeDelete = controller.Get().Count();

            controller.Delete(1);

            var afterDelete = controller.Get().Count();
            Assert.Equal(beforeDelete - 1, afterDelete);
        }
    }
}
