using System.ComponentModel;

namespace EnterpriseChat.Application.Tools;

public static class WeatherTool
{
    [Description("Gets the current weather for a city.")]
    public static string GetWeather([Description("The name of the city.")]string city)
    {
        return city.ToLowerInvariant()
        ?? throw new ArgumentNullException(nameof(city))
        switch
        {
            "bangalore" or "bengaluru" =>
                "Bangalore is 27°C and partly cloudy.",

            "london" =>
                "London is 15°C and cloudy.",

            "new york" =>
                "New York is 22°C and sunny.",

            _ =>
                $"{city} is 25°C and partly cloudy."
        };
    }
}