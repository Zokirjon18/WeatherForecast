using Newtonsoft.Json;

namespace WeatherForecastApp.Models
{
    // Cloud data
    public class CloudData
    {
        [JsonProperty("all")]
        public int All { get; set; }
    }
}