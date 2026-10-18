using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;
using soa_ca1.Models;

namespace soa_ca1.Services
{
    // API docs: https://open-meteo.com/en/docs
    public class WeatherService
    {
        public static readonly string BASE_URL = "https://api.open-meteo.com/v1/";
        private static readonly RestClient client = new(BASE_URL);

        public static async Task<List<HourlyForecast>> GetForecastAsync(double latitude, double longitude)
        {
            var request = new RestRequest("forecast");
            request.AddParameter("latitude", latitude.ToString(CultureInfo.InvariantCulture));
            request.AddParameter("longitude", longitude.ToString(CultureInfo.InvariantCulture));
            request.AddParameter("hourly", "temperature_2m,precipitation_probability,weather_code");
            request.AddParameter("timezone", "Europe/Dublin");
            request.AddParameter("forecast_days", 2);

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                throw new HttpRequestException("Couldn't reach the weather service.", response.ErrorException);
            }
            return ParseForecast(response.Content);
        }

        public static List<HourlyForecast> ParseForecast(string json)
        {
            var forecasts = new List<HourlyForecast>();
            OpenMeteoHourly? hourly = JsonSerializer.Deserialize<OpenMeteoResponse>(json)?.Hourly;
            if (hourly is null)
            {
                return forecasts;
            }

            for (int i = 0; i < hourly.Time.Count; i++)
            {
                forecasts.Add(new HourlyForecast
                {
                    Time = DateTime.Parse(hourly.Time[i], CultureInfo.InvariantCulture),
                    Temperature = hourly.Temperature.ElementAtOrDefault(i) ?? 0,
                    RainChance = hourly.RainChance.ElementAtOrDefault(i) ?? 0,
                    WeatherCode = hourly.WeatherCode.ElementAtOrDefault(i) ?? 0
                });
            }
            return forecasts;
        }

        public static HourlyForecast? FindForecast(List<HourlyForecast> forecasts, string arrivalTime, DateTime now)
        {
            if (!TimeSpan.TryParse(arrivalTime, CultureInfo.InvariantCulture, out TimeSpan time))
            {
                return null;
            }

            int hour = (int)Math.Round(time.TotalHours, MidpointRounding.AwayFromZero) % 24;
            DateTime currentHour = now.Date.AddHours(now.Hour);
            return forecasts.Find(f => f.Time >= currentHour && f.Time.Hour == hour);
        }
    }

    public class OpenMeteoResponse
    {
        [JsonPropertyName("hourly")]
        public OpenMeteoHourly? Hourly { get; set; }
    }

    public class OpenMeteoHourly
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        [JsonPropertyName("temperature_2m")]
        public List<double?> Temperature { get; set; } = new();

        [JsonPropertyName("precipitation_probability")]
        public List<int?> RainChance { get; set; } = new();

        [JsonPropertyName("weather_code")]
        public List<int?> WeatherCode { get; set; } = new();
    }
}
