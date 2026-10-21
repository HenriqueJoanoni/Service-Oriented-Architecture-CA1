using soa_ca1.Models;
using soa_ca1.Services;

namespace soa_ca1.Tests;

public class IrishRailParsingTests
{
    private static string ReadSample(string fileName)
    {
        return File.ReadAllText(Path.Combine("SampleData", fileName));
    }

    [Fact]
    public void ParseDepartures_SkipsTrainsTerminatingAtStation()
    {
        var departures = IrishRailService.ParseDepartures(ReadSample("departures.xml"));

        Assert.Equal(2, departures.Count);
        Assert.DoesNotContain(departures, d => d.TrainCode == "D920");
    }

    [Fact]
    public void ParseDepartures_ReadsAllFields()
    {
        var departure = IrishRailService.ParseDepartures(ReadSample("departures.xml"))[0];

        Assert.Equal("A108", departure.TrainCode);
        Assert.Equal("14 Oct 2026", departure.TrainDate);
        Assert.Equal("Belfast", departure.Destination);
        Assert.Equal("Northbound", departure.Direction);
        Assert.Equal("10:55", departure.ExpectedDeparture);
        Assert.Equal(11, departure.DueIn);
        Assert.Equal(4, departure.Late);
    }

    [Fact]
    public void ParseStations_SortsByName()
    {
        var stations = IrishRailService.ParseStations(ReadSample("stations.xml"));

        Assert.Equal(new[] { "Belfast", "Drogheda", "Dundalk" }, stations.Select(s => s.Name));
        Assert.Equal("BFSTC", stations[0].Code);
    }

    [Theory]
    [InlineData(4, "En Route", "+4 min")]
    [InlineData(0, "En Route", "On time")]
    [InlineData(0, "No Information", "Scheduled")]
    public void StatusLabel_DependsOnLatenessAndStatus(int late, string status, string expected)
    {
        var departure = new Departure { Late = late, Status = status };

        Assert.Equal(expected, departure.StatusLabel);
    }
}
