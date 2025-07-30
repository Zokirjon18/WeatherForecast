using WeatherForecastApp.Enums;

namespace WeatherForecastApp.BotHandler.WeatherForecastBot.WeatherSessions;

public class Session
{
    public long ChatId { get; set; }
    public Step Step { get; set; }
    public Mode Mode { get; set; } = Mode.None;
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
}
