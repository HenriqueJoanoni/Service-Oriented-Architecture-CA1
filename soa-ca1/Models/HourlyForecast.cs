namespace soa_ca1.Models
{
    public class HourlyForecast
    {
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
        public int RainChance { get; set; }
        public int WeatherCode { get; set; }

        public bool NeedsUmbrella => RainChance >= 50;

        public string Icon
        {
            get
            {
                switch (WeatherCode)
                {
                    case 0:
                    {
                        return "bi-sun";
                    }
                    case 1 or 2:
                    {
                        return "bi-cloud-sun";
                    }
                    case 3:
                    {
                        return "bi-cloud";
                    }
                    case 45 or 48:
                    {
                        return "bi-cloud-fog";
                    }
                    case >= 51 and <= 57:
                    {
                        return "bi-cloud-drizzle";
                    }
                    case >= 61 and <= 67 or >= 80 and <= 82:
                    {
                        return "bi-cloud-rain";
                    }
                    case >= 71 and <= 77 or 85 or 86:
                    {
                        return "bi-cloud-snow";
                    }
                    case >= 95 and <= 99:
                    {
                        return "bi-cloud-lightning-rain";
                    }
                    default:
                    {
                        return "bi-question-circle";
                    }
                }
            }
        }

        public string Description
        {
            get
            {
                switch (WeatherCode)
                {
                    case 0:
                    {
                        return "Clear sky";
                    }
                    case 1 or 2:
                    {
                        return "Partly cloudy";
                    }
                    case 3:
                    {
                        return "Overcast";
                    }
                    case 45 or 48:
                    {
                        return "Fog";
                    }
                    case >= 51 and <= 57:
                    {
                        return "Drizzle";
                    }
                    case >= 61 and <= 67:
                    {
                        return "Rain";
                    }
                    case >= 71 and <= 77:
                    {
                        return "Snow";
                    }
                    case >= 80 and <= 82:
                    {
                        return "Rain showers";
                    }
                    case 85 or 86:
                    {
                        return "Snow showers";
                    }
                    case >= 95 and <= 99:
                    {
                        return "Thunderstorm";
                    }
                    default:
                    {
                        return "Unknown";
                    }
                }
            }
        }
    }
}
