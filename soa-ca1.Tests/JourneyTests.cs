using soa_ca1.Models;
using soa_ca1.Services;

namespace soa_ca1.Tests;

public class JourneyTests
{
    private static List<TrainStop> SampleStops()
    {
        return IrishRailService.ParseTrainStops(File.ReadAllText(Path.Combine("SampleData", "movements.xml")));
    }

    [Fact]
    public void BuildJourney_DestinationAfterDeparture_ReturnsArrivalTime()
    {
        var departure = new Departure { TrainCode = "A154" };

        Journey? journey = IrishRailService.BuildJourney(departure, SampleStops(), "DGHDA", "DDALK");

        Assert.NotNull(journey);
        Assert.Equal("15:49", journey.ArrivalTime);
        Assert.Same(departure, journey.Departure);
    }

    [Fact]
    public void BuildJourney_DestinationBeforeDeparture_ReturnsNull()
    {
        Assert.Null(IrishRailService.BuildJourney(new Departure(), SampleStops(), "DDALK", "DGHDA"));
    }

    [Fact]
    public void BuildJourney_DestinationIsTimingPoint_ReturnsNull()
    {
        Assert.Null(IrishRailService.BuildJourney(new Departure(), SampleStops(), "CNLLY", "SUBJN"));
    }

    [Fact]
    public async Task GetJourneysAsync_SameStation_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => IrishRailService.GetJourneysAsync("DDALK", "DDALK", 60));
    }
}
