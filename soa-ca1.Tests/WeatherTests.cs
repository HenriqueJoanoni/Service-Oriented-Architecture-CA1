using System.Globalization;
using System.Text.Json;
using soa_ca1.Models;
using soa_ca1.Services;

namespace soa_ca1.Tests;

public class WeatherTests
{
    private static List<HourlyForecast> SampleForecast()
    {
        return WeatherService.ParseForecast(File.ReadAllText(Path.Combine("SampleData", "forecast.json")));
    }

    [Fact]
    public void ParseForecast_MalformedJson_Throws()
    {
        Assert.ThrowsAny<JsonException>(() => WeatherService.ParseForecast("{\"hourly\": "));
    }

    [Theory]
    [InlineData("17:49", 18)]
    [InlineData("17:29", 17)]
    public void FindForecast_RoundsToNearestHour(string arrival, int expectedHour)
    {
        var now = new DateTime(2026, 10, 18, 16, 50, 0);

        HourlyForecast? forecast = WeatherService.FindForecast(SampleForecast(), arrival, now);

        Assert.NotNull(forecast);
        Assert.Equal(new DateTime(2026, 10, 18, expectedHour, 0, 0), forecast.Time);
    }

    [Fact]
    public void FindForecast_ArrivalAfterMidnight_UsesNextDay()
    {
        var now = new DateTime(2026, 10, 18, 23, 20, 0);

        HourlyForecast? forecast = WeatherService.FindForecast(SampleForecast(), "00:10", now);

        Assert.Equal(new DateTime(2026, 10, 19, 0, 0, 0), forecast?.Time);
    }

    [Theory]
    [InlineData(49, false)]
    [InlineData(50, true)]
    public void NeedsUmbrella_FromFiftyPercent(int rainChance, bool expected)
    {
        Assert.Equal(expected, new HourlyForecast { RainChance = rainChance }.NeedsUmbrella);
    }

    [Fact]
    public void ParseStations_ReadsCoordinates_OnNonEnglishMachine()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");

            Station dundalk = IrishRailService.ParseStations(File.ReadAllText(Path.Combine("SampleData", "stations.xml")))
                .Find(s => s.Code == "DDALK")!;

            Assert.Equal(54.0009, dundalk.Latitude);
            Assert.Equal(-6.41302, dundalk.Longitude);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
