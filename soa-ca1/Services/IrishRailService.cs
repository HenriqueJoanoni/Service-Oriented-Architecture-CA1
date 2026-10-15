using System.Xml.Linq;
using RestSharp;
using soa_ca1.Models;

namespace soa_ca1.Services
{
    // API docs: https://api.irishrail.ie/realtime/
    public class IrishRailService
    {
        public static readonly string BASE_URL = "https://api.irishrail.ie/realtime/realtime.asmx/";
        private static readonly XNamespace ns = "http://api.irishrail.ie/realtime/";
        private static readonly RestClient client = new(BASE_URL);

        public static async Task<List<Station>> GetStationsAsync()
        {
            var request = new RestRequest("getAllStationsXML");
            string xml = await GetXmlAsync(request);
            return ParseStations(xml);
        }

        public static async Task<List<Departure>> GetDeparturesAsync(string stationCode, int minutes)
        {
            if (string.IsNullOrWhiteSpace(stationCode))
                throw new ArgumentException("Choose a station first.", nameof(stationCode));
            if (minutes < 5 || minutes > 90)
                throw new ArgumentOutOfRangeException(nameof(minutes), "The API only accepts 5 to 90 minutes.");

            var request = new RestRequest("getStationDataByCodeXML_WithNumMins");
            request.AddParameter("StationCode", stationCode);
            request.AddParameter("NumMins", minutes);
            string xml = await GetXmlAsync(request);
            return ParseDepartures(xml);
        }

        public static List<Station> ParseStations(string xml)
        {
            var stations = new List<Station>();
            foreach (XElement item in XDocument.Parse(xml).Descendants(ns + "objStation"))
            {
                stations.Add(new Station
                {
                    Code = Text(item, "StationCode"),
                    Name = Text(item, "StationDesc")
                });
            }
            stations.Sort();
            return stations;
        }

        public static List<Departure> ParseDepartures(string xml)
        {
            var departures = new List<Departure>();
            foreach (XElement item in XDocument.Parse(xml).Descendants(ns + "objStationData"))
            {
                // "D" = the train terminates at this station, so it never departs from it
                if (Text(item, "Locationtype") == "D") continue;

                departures.Add(new Departure
                {
                    TrainCode = Text(item, "Traincode"),
                    TrainType = Text(item, "Traintype"),
                    Origin = Text(item, "Origin"),
                    Destination = Text(item, "Destination"),
                    Direction = Text(item, "Direction"),
                    Status = Text(item, "Status"),
                    LastLocation = Text(item, "Lastlocation"),
                    ScheduledDeparture = Text(item, "Schdepart"),
                    ExpectedDeparture = Text(item, "Expdepart"),
                    DueIn = Number(item, "Duein"),
                    Late = Number(item, "Late")
                });
            }
            departures.Sort();
            return departures;
        }

        private static async Task<string> GetXmlAsync(RestRequest request)
        {
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                throw new HttpRequestException("Couldn't reach Irish Rail. Try again in a moment.", response.ErrorException);
            }
            return response.Content;
        }

        // The API pads some values with spaces (e.g. "A108 ")
        private static string Text(XElement item, string name)
        {
            return item.Element(ns + name)?.Value.Trim() ?? string.Empty;
        }

        private static int Number(XElement item, string name)
        {
            return int.TryParse(Text(item, name), out int value) ? value : 0;
        }
    }
}
