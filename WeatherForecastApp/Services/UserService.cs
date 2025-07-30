using Newtonsoft.Json;
using WeatherForecastApp.Helpers;
using WeatherForecastApp.Domain;
using WeatherForecastApp.Paths;

namespace WeatherForecastApp.Services;

public class UserService
{
    public bool Register(long chatId, string countryCode, string city)
    {
        List<Foydalanuvchi> users = DeserializeUsers();

        if(users is null)
        {
            users = new List<Foydalanuvchi>();
        }

        if (users.Any(u => u.Id == chatId))
        {
            return false;
        }

        if (countryCode == null)
        {
            throw new Exception("Country must be entered!");
        }
        if (city == null)
        {
            throw new Exception("City must be entered!");
        }

        
        users.Add(new Foydalanuvchi
        {
            Id = chatId,
            CountryCode = countryCode,
            City = city
        });

        FileHelper.WriteToFile(PathHolder.Users, users);

        return true;
    }

    public bool Login(long chatId, string countryCode, string city)
    {
        List<Foydalanuvchi> users = DeserializeUsers();

        Foydalanuvchi user = users.FirstOrDefault(users => users.Id == chatId);

        if (user == null)
            return false;

        if (countryCode == null)
        {
            throw new Exception("Country must be entered!");
        }
        if (city == null)
        {
            throw new Exception("City must be entered!");
        }

        return true;
    }

    public Foydalanuvchi Get(long chatId)
    {
        List<Foydalanuvchi> users = DeserializeUsers();

        Foydalanuvchi? user = users.FirstOrDefault(u => u.Id == chatId);

        if (user == null)
        {
            return null;
        }

        return user;
    }

    private List<Foydalanuvchi> DeserializeUsers()
    {
        string rawContent = File.ReadAllText(PathHolder.Users);

        List<Foydalanuvchi>? users = JsonConvert.DeserializeObject<List<Foydalanuvchi>>(rawContent);

        return users;
    }
}
