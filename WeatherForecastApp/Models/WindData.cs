using Newtonsoft.Json;

namespace WeatherForecastApp.Models
{
    // Wind data
    public class WindData
    {
        [JsonProperty("speed")]
        public double Speed { get; set; }

        [JsonProperty("deg")]
        public int Deg { get; set; }

        [JsonProperty("gust")]
        public double? Gust { get; set; }
    }
}