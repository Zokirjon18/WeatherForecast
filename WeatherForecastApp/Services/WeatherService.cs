using Newtonsoft.Json;
using WeatherForecastApp.Models;

namespace WeatherForecastApp.Services;

public class WeatherService
{
    private readonly string apiKey = "2c6e6a70646d1deadd30125b4ec24eb1";
    private HttpClient httpClient = new HttpClient();

    public async Task<(bool isSuccessfull, double lat, double lon)> GetCoordinates(
        string countryCode,
        string city)
    {
        try
        {
            var query = $"{city},{countryCode}".Trim(',');
            var url = $"https://api.openweathermap.org/geo/1.0/direct?q={query}&limit=1&appid={apiKey}";

            Console.WriteLine($"🌍 Geocoding URL: {url}");

            HttpResponseMessage responseMessage = await httpClient.GetAsync(url);

            if (responseMessage.IsSuccessStatusCode)
            {
                string content = await responseMessage.Content.ReadAsStringAsync();
                Console.WriteLine($"📍 Geocoding response: {content}");

                // Geocoding API returns array of coordinates
                List<Coordinate> coordinates = JsonConvert.DeserializeObject<List<Coordinate>>(content);

                if (coordinates != null && coordinates.Count > 0)
                {
                    var coordinate = coordinates[0];
                    Console.WriteLine($"✅ Found coordinates: {coordinate.Latitude}, {coordinate.Longitude}");
                    return (true, coordinate.Latitude, coordinate.Longitude);
                }
                else
                {
                    Console.WriteLine($"❌ No coordinates found for {city}, {countryCode}");
                    return (false, 0, 0);
                }
            }
            else
            {
                string errorContent = await responseMessage.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Geocoding API failed: {responseMessage.StatusCode} - {errorContent}");
                return (false, 0, 0);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GetCoordinates: {ex.Message}");
            return (false, 0, 0);
        }
    }

    public async Task<WeatherForecast> GetWeather(double lat, double lon)
    {
        try
        {
            // Use the current weather API (free tier)
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&units=metric&appid={apiKey}";

            Console.WriteLine($"🌤️ Weather URL: {url}");

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string content = await httpResponseMessage.Content.ReadAsStringAsync();
                Console.WriteLine($"🌡️ Weather response: {content.Substring(0, Math.Min(300, content.Length))}...");

                WeatherForecast weatherForecast = JsonConvert.DeserializeObject<WeatherForecast>(content);

                if (weatherForecast != null)
                {
                    Console.WriteLine($"✅ Weather data parsed successfully for {weatherForecast.Name}");
                    return weatherForecast;
                }
                else
                {
                    Console.WriteLine($"❌ Failed to parse weather data");
                    return null;
                }
            }
            else
            {
                string errorContent = await httpResponseMessage.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Weather API failed: {httpResponseMessage.StatusCode} - {errorContent}");
                return null;
            }
        }
        catch (JsonSerializationException jsonEx)
        {
            Console.WriteLine($"❌ JSON Error in GetWeather: {jsonEx.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GetWeather: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        httpClient?.Dispose();
    }
}