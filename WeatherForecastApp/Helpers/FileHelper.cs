using Newtonsoft.Json;
using Telegram.Bot.Types;
using WeatherForecastApp.Paths;

namespace WeatherForecastApp.Helpers
{
    public static class FileHelper
    {
        public static bool CheckUserExistence(long chatId)
        {
            if (!File.Exists(PathHolder.Users))
            {
                File.Create(PathHolder.Users).Close();
            }

            string rawContent = File.ReadAllText(PathHolder.Users);
            List<User> users = JsonConvert.DeserializeObject<List<User>>(rawContent);
            if (users is not null)
                return users.Any(u => u.Id == chatId);

            return false;
        }
        public static void WriteToFile<T>(string filePath, List<T> models)
        {
            string content = JsonConvert.SerializeObject(models, Formatting.Indented);

            File.WriteAllText(filePath, content);
        }
    }
}
