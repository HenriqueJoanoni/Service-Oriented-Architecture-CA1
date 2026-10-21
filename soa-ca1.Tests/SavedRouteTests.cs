using System.Text.Json;
using soa_ca1.Models;
using soa_ca1.Services;

namespace soa_ca1.Tests;

public class SavedRouteTests
{
    private static SavedRoute Route(string name, string fromCode, string toCode)
    {
        return new SavedRoute { Name = name, FromCode = fromCode, ToCode = toCode };
    }

    [Fact]
    public void Equals_SameStationsDifferentName_AreEqual()
    {
        SavedRoute a = Route("Home → DkIT", "DGHDA", "DDALK");
        SavedRoute b = Route("College", "DGHDA", "DDALK");

        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_ReversedDirection_AreNotEqual()
    {
        Assert.True(Route("Home → DkIT", "DGHDA", "DDALK") != Route("DkIT → Home", "DDALK", "DGHDA"));
    }

    [Fact]
    public void Add_Duplicate_ReturnsFalseAndKeepsList()
    {
        var routes = new List<SavedRoute> { Route("Home → DkIT", "DGHDA", "DDALK") };

        bool added = RouteStore.Add(routes, Route("Another name", "DGHDA", "DDALK"));

        Assert.False(added);
        Assert.Single(routes);
    }

    [Fact]
    public void SaveThenLoad_KeepsRoutesSortedByName()
    {
        string path = Path.GetTempFileName();
        try
        {
            RouteStore.Save(new List<SavedRoute> { Route("Work", "A", "B"), Route("Beach", "C", "D") }, path);

            List<SavedRoute> loaded = RouteStore.Load(path);

            Assert.Equal(new[] { "Beach", "Work" }, loaded.Select(r => r.Name));
            Assert.Equal("C", loaded[0].FromCode);
            Assert.Equal("D", loaded[0].ToCode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_CorruptFile_Throws()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "[{ \"Name\": ");

            Assert.ThrowsAny<JsonException>(() => RouteStore.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
