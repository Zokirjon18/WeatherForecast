using Newtonsoft.Json;

namespace WeatherForecastApp.Models
{
    // Main model for the current weather API response
    public class WeatherForecast
    {
        [JsonProperty("coord")]
        public Coordinate Coord { get; set; }

        [JsonProperty("weather")]
        public List<WeatherDescription> Weather { get; set; }

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("main")]
        public MainWeatherData Main { get; set; }

        [JsonProperty("visibility")]
        public int Visibility { get; set; }

        [JsonProperty("wind")]
        public WindData Wind { get; set; }

        [JsonProperty("clouds")]
        public CloudData Clouds { get; set; }

        [JsonProperty("dt")]
        public long Dt { get; set; }

        [JsonProperty("sys")]
        public SystemData Sys { get; set; }

        [JsonProperty("timezone")]
        public int Timezone { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cod")]
        public int Cod { get; set; }

        // Helper property to get temperature easily
        public double Temperature => Main?.Temp ?? 0;

        // Helper property to get description
        public string Description => Weather?.FirstOrDefault()?.Description ?? "N/A";
    }
}