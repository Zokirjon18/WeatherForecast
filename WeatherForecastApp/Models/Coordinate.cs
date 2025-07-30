using Newtonsoft.Json;

namespace WeatherForecastApp.Models;

public class Coordinate
{
    [JsonProperty("lon")]
    public float Longitude { get; set; } 

    [JsonProperty("lat")]
    public float Latitude { get; set; }
}