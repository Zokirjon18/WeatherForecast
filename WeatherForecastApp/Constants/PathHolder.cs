namespace WeatherForecastApp.Paths
{
    internal class PathHolder
    {
        public static string GetProjectRoot()
        {
            var current = Directory.GetCurrentDirectory();
            while (!Directory.GetFiles(current, "*.csproj").Any())
            {
                current = Directory.GetParent(current).FullName;
            }
            return current;
        }

        public static readonly string Users = Path.Combine(GetProjectRoot(), "Data","users.json");
        public static readonly string Weather = Path.Combine(GetProjectRoot(), "Data", "weather.json");
    }
}
