namespace WeatherForecastApp.Models
{
    // Main model for the current weather API response
    public class WeatherResponse
    {
        public List<WeatherForecast> List { get; set; }
    }
}