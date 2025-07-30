using Newtonsoft.Json;

namespace WeatherForecastApp.Models
{
    // System data
    public class SystemData
    {
        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("sunrise")]
        public long Sunrise { get; set; }

        [JsonProperty("sunset")]
        public long Sunset { get; set; }
    }
}