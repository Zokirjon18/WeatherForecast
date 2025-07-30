using System.Text.Json.Serialization;
using Newtonsoft.Json;
using WeatherForecastApp.BotHandler.WeatherForecastBot.UIService;
using WeatherForecastApp.Models;

namespace WeatherForecastApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //HttpClient client = new HttpClient();
            //string daily = "https://api.openweathermap.org/data/2.5/weather?q=Tashkent,UZ&appid=2c6e6a70646d1deadd30125b4ec24eb1&units=metric\r\n";

            //string weekly = "https://api.openweathermap.org/data/2.5/onecall?lat=41.3123363&lon=69.2787079&exclude=minutely,hourly&appid=2c6e6a70646d1deadd30125b4ec24eb1&units=metric\r\n";

            //HttpResponseMessage responseMessage = await client.GetAsync(weekly);

            //if (responseMessage.IsSuccessStatusCode)
            //{
            //    string content = await responseMessage.Content.ReadAsStringAsync();

            //    dynamic weatherInfo = JsonConvert.DeserializeObject<dynamic>(content);

            //    Console.Write(weatherInfo);

            ////https://api.openweathermap.org/data/2.5/weather?q=London&appid=2c6e6a70646d1deadd30125b4ec24eb1
            //}

            WeatherForecastBotUI weatherForecastBotUI = new WeatherForecastBotUI();

            weatherForecastBotUI.StartAsync();
        }
    }
}
