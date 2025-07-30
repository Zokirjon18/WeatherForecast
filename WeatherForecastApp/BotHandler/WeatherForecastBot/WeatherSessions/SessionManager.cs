namespace WeatherForecastApp.BotHandler.WeatherForecastBot.WeatherSessions;

public static class SessionManager
{
    private static Dictionary<long, Session> _sessions = new();

    public static Session GetOrCreateSession(long chatId)
    {
        if (!_sessions.ContainsKey(chatId))
            _sessions[chatId] = new Session { ChatId = chatId };

        return _sessions[chatId];
    }

    public static void ClearSession(long chatId)
    {
        _sessions.Remove(chatId);
    }
}