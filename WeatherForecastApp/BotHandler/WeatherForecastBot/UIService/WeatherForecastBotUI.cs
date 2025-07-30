using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WeatherForecastApp.BotHandler.WeatherForecastBot.WeatherSessions;
using WeatherForecastApp.Domain;
using WeatherForecastApp.Enums;
using WeatherForecastApp.Helpers;
using WeatherForecastApp.Models;
using WeatherForecastApp.Services;

namespace WeatherForecastApp.BotHandler.WeatherForecastBot.UIService
{
    public class WeatherForecastBotUI
    {
        private CancellationTokenSource _cts;
        private UserService userService;
        private WeatherService weatherService;

        public WeatherForecastBotUI()
        {
            userService = new UserService();
            weatherService = new WeatherService();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            ITelegramBotClient client = new TelegramBotClient("YOUR_TELEGRAM_BOT_TOKEN");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            client.StartReceiving(
                HandleUpdateAsync,
                HandlePollingErrorAsync,
                receiverOptions: new ReceiverOptions
                {
                    AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
                },
                cancellationToken: _cts.Token
            );

            Console.WriteLine("🤖 Bot is running... Press any key to stop.");
            Console.ReadKey();
        }

        public async Task StopAsync()
        {
            if (_cts != null)
            {
                await _cts.CancelAsync();
                _cts.Dispose();
                _cts = null;
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            try
            {
                Task handler = default;

                if (update.Message != null)
                {
                    handler = OnMessageReceived(client, update.Message, cancellationToken);
                }
                else if (update.CallbackQuery != null)
                {
                    handler = OnCallbackQueryReceived(client, update.CallbackQuery, cancellationToken);
                }

                if (handler != null)
                    await handler;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(client, ex, update);
            }
        }

        private async Task OnMessageReceived(ITelegramBotClient client, Message message, CancellationToken cancellationToken)
        {
            if (message.Text is null) return;

            long chatId = message.Chat.Id;
            string text = message.Text.Trim();
            Session session = SessionManager.GetOrCreateSession(chatId);

            // Handle start/restart commands
            if (text.ToLower() == "/start" || text.ToLower() == "/restart")
            {
                await StartCommandAsync(client, chatId, text, session, cancellationToken);
            }
            // Handle weather commands - FIXED: Check user exists first
            else if (text.ToLower() == "/today" || text.ToLower() == "/week")
            {
                await HandleWeatherCommandAsync(client, chatId, text, cancellationToken);
            }
            // Handle ongoing registration/login process
            else if (session.Mode != Mode.None && session.Step != Step.Done)
            {
                await DefineRegionInfoAsync(client, chatId, text, session, cancellationToken);
            }
            // Handle unknown commands
            else
            {
                await client.SendMessage(chatId,
                    "Sorry, I didn't understand that! Use /start to begin or /today to get weather.",
                    cancellationToken: cancellationToken);
            }
        }

        // NEW METHOD: Separate weather command handling with proper error checking
        private async Task HandleWeatherCommandAsync(ITelegramBotClient client, long chatId, string command, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user exists and is registered
                if (!FileHelper.CheckUserExistence(chatId))
                {
                    await client.SendMessage(
                        chatId,
                        "❌ You need to register first! Use /start to begin registration.",
                        cancellationToken: cancellationToken);
                    return;
                }

                // Get user data
                Foydalanuvchi user = userService.Get(chatId);

                // Validate user data
                if (user == null || string.IsNullOrEmpty(user.CountryCode) || string.IsNullOrEmpty(user.City))
                {
                    await client.SendMessage(
                        chatId,
                        "❌ Your profile is incomplete. Please use /restart to update your information.",
                        cancellationToken: cancellationToken);
                    return;
                }

                // Get coordinates
                var coordinateResult = await weatherService.GetCoordinates(countryCode: user.CountryCode, city: user.City);

                if (!coordinateResult.isSuccessfull)
                {
                    await client.SendMessage(
                        chatId,
                        $"❌ Sorry, we couldn't find weather data for {user.City}, {user.CountryCode}. " +
                        $"Please check your location settings with /restart",
                        cancellationToken: cancellationToken);
                    return;
                }

                // Get weather data
                WeatherForecast weather = await weatherService.GetWeather(coordinateResult.lat, coordinateResult.lon);

                if (weather == null)
                {
                    await client.SendMessage(
                        chatId,
                        "❌ Weather service is currently unavailable. Please try again later.",
                        cancellationToken: cancellationToken);
                    return;
                }

                // Format and send weather information
                if (command.ToLower() == "/today")
                {
                    await SendTodayWeatherAsync(client, chatId, weather, user, cancellationToken);
                }
                else if (command.ToLower() == "/week")
                {
                    await SendWeeklyWeatherAsync(client, chatId, weather, user, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleWeatherCommandAsync: {ex}");
                await client.SendMessage(
                    chatId,
                    "❌ An error occurred while fetching weather data. Please try again later.",
                    cancellationToken: cancellationToken);
            }
        }

        // NEW METHOD: Format and send today's weather
        private async Task SendTodayWeatherAsync(ITelegramBotClient client, long chatId, WeatherForecast weather, Foydalanuvchi user, CancellationToken cancellationToken)
        {
            try
            {
                // Format the date properly
                DateTime weatherDate = DateTimeOffset.FromUnixTimeSeconds(weather.Dt).DateTime;

                string weatherMessage = $"🌤️ **Today's Weather for {weather.Name}, {weather.Sys.Country}**\n\n" +
                                      $"📅 Date: {weatherDate:MMM dd, yyyy HH:mm}\n" +
                                      $"🌡️ Temperature: {weather.Main.Temp}°C\n" +
                                      $"🌡️ Feels like: {weather.Main.FeelsLike}°C\n" +
                                      $"🌡️ Min/Max: {weather.Main.TempMin}°C / {weather.Main.TempMax}°C\n" +
                                      $"💧 Humidity: {weather.Main.Humidity}%\n" +
                                      $"🌬️ Pressure: {weather.Main.Pressure} hPa\n" +
                                      $"💨 Wind: {weather.Wind.Speed} m/s\n" +
                                      $"☁️ Condition: {weather.Description}\n" +
                                      $"👁️ Visibility: {weather.Visibility / 1000} km";

                await client.SendMessage(
                    chatId,
                    weatherMessage,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error formatting weather message: {ex}");
                // Fallback to simple message
                await client.SendMessage(
                    chatId,
                    $"🌤️ Today's temperature in {weather.Name}: {weather.Main.Temp}°C\n" +
                    $"Condition: {weather.Description}",
                    cancellationToken: cancellationToken);
            }
        }

        // NEW METHOD: Placeholder for weekly weather (you can implement this later)
        private async Task SendWeeklyWeatherAsync(ITelegramBotClient client, long chatId, WeatherForecast weather, Foydalanuvchi user, CancellationToken cancellationToken)
        {
            await client.SendMessage(
                chatId,
                "📅 Weekly forecast feature is coming soon! Use /today for current weather.",
                cancellationToken: cancellationToken);
        }

        public async Task StartCommandAsync(ITelegramBotClient client, long chatId, string text, Session session, CancellationToken cancellationToken)
        {
            // Reset session
            session.Step = Step.Country;
            session.Mode = Mode.None; // Reset mode first

            if (FileHelper.CheckUserExistence(chatId))
            {
                session.Mode = Mode.Login;
                await client.SendMessage(
                    chatId,
                    "🔄 **Updating your information**\n\n" +
                    "First, let's enter your 2-letter country code (e.g., UZ for Uzbekistan):",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
            }
            else
            {
                session.Mode = Mode.Register;
                await client.SendMessage(
                    chatId,
                    "☁️ **Welcome to Weather Forecast Bot!** 🌤️\n\n" +
                    "Let's get you set up. First, enter your 2-letter country code (e.g., UZ for Uzbekistan):",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
            }
        }

        public async Task DefineRegionInfoAsync(ITelegramBotClient client, long chatId, string text, Session session, CancellationToken cancellationToken)
        {
            try
            {
                if (session.Step == Step.Country)
                {
                    // Validate country code format
                    if (string.IsNullOrWhiteSpace(text) || text.Length != 2)
                    {
                        await client.SendMessage(
                            chatId,
                            "❌ Please enter a valid 2-letter country code (e.g., UZ, US, GB):",
                            cancellationToken: cancellationToken);
                        return;
                    }

                    session.Country = text.ToUpper(); // Store in uppercase
                    session.Step = Step.City;

                    await client.SendMessage(
                        chatId,
                        "✅ Great! Now enter your city name:",
                        cancellationToken: cancellationToken);
                }
                else if (session.Step == Step.City)
                {
                    // Validate city name
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        await client.SendMessage(
                            chatId,
                            "❌ Please enter a valid city name:",
                            cancellationToken: cancellationToken);
                        return;
                    }

                    session.City = text.Trim();
                    session.Step = Step.Done;
                    bool success = false;

                    if (session.Mode == Mode.Login)
                    {
                        success = userService.Login(chatId, session.Country, session.City);
                    }
                    else if (session.Mode == Mode.Register)
                    {
                        success = userService.Register(chatId, session.Country, session.City);
                    }

                    if (success)
                    {
                        await client.SendMessage(
                            chatId,
                            "🎉 **Perfect! You're all set up!** ✅\n\n" +
                            "Now you can use:\n" +
                            "• /today - Get today's weather\n" +
                            "• /week - Get weekly forecast (coming soon)\n" +
                            "• /restart - Update your location",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken);

                        // Reset session
                        session.Mode = Mode.None;
                        session.Step = Step.Done;
                    }
                    else
                    {
                        await client.SendMessage(
                            chatId,
                            "❌ Sorry, something went wrong while processing your information. Please try /restart",
                            cancellationToken: cancellationToken);

                        // Reset session on failure
                        session.Mode = Mode.None;
                        session.Step = Step.Done;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DefineRegionInfoAsync: {ex}");
                await client.SendMessage(
                    chatId,
                    "❌ An error occurred. Please try /restart",
                    cancellationToken: cancellationToken);
            }
        }

        private async Task OnCallbackQueryReceived(ITelegramBotClient client, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is null) return;

            // Answer the callback query to remove loading state
            await client.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
        }

        private async Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);

            // Add delay to prevent rapid retries
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        private async Task HandleErrorAsync(ITelegramBotClient client, Exception exception, Update? update = null)
        {
            Console.WriteLine($"An error occurred while processing update {update?.Id}: {exception}");
        }
    }
}
