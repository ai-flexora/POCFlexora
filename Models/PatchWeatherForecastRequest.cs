using System.ComponentModel.DataAnnotations;

namespace POCFlexora.Models
{
    public class PatchWeatherForecastRequest
    {
        public DateOnly? Date { get; set; }

        [Range(-273, 1000, ErrorMessage = "TemperatureC must be between -273 and 1000.")]
        public int? TemperatureC { get; set; }

        [MaxLength(100, ErrorMessage = "Summary cannot exceed 100 characters.")]
        public string? Summary { get; set; }
    }
}
