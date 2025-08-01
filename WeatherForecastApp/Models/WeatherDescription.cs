﻿using Newtonsoft.Json;

namespace WeatherForecastApp.Models
{
    // Weather description
    public class WeatherDescription
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("main")]
        public string Main { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
}