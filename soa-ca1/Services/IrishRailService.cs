using System.Globalization;
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
        private static List<Station>? cachedStations;

        // The station list practically never changes, so it's only downloaded once
        public static async Task<List<Station>> GetStationsAsync()
        {
            if (cachedStations is null)
            {
                var request = new RestRequest("getAllStationsXML");
                string xml = await GetXmlAsync(request);
                cachedStations = ParseStations(xml);
            }
            return cachedStations;
        }

        public static async Task<List<Departure>> GetDeparturesAsync(string stationCode, int minutes)
        {
            if (string.IsNullOrWhiteSpace(stationCode))
            {
                throw new ArgumentException("Choose a station first.");   
            }

            if (minutes < 5 || minutes > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), "The API only accepts 5 to 90 minutes.");
            }

            var request = new RestRequest("getStationDataByCodeXML_WithNumMins");
            request.AddParameter("StationCode", stationCode);
            request.AddParameter("NumMins", minutes);
            string xml = await GetXmlAsync(request);
            return ParseDepartures(xml);
        }

        public static async Task<List<TrainStop>> GetTrainStopsAsync(string trainCode, string trainDate)
        {
            var request = new RestRequest("getTrainMovementsXML");
            request.AddParameter("TrainId", trainCode);
            request.AddParameter("TrainDate", trainDate);
            string xml = await GetXmlAsync(request);
            return ParseTrainStops(xml);
        }

        public static async Task<List<Journey>> GetJourneysAsync(string fromCode, string toCode, int minutes)
        {
            if (string.IsNullOrWhiteSpace(toCode))
            {
                throw new ArgumentException("Choose where you're going.");
            }

            if (fromCode == toCode)
            {
                throw new ArgumentException("Choose two different stations.");
            }

            List<Departure> departures = await GetDeparturesAsync(fromCode, minutes);

            // One request per train, so they run at the same time instead of one after the other
            List<TrainStop>[] allStops = await Task.WhenAll(departures.Select(d => GetTrainStopsAsync(d.TrainCode, d.TrainDate)));

            var journeys = new List<Journey>();
            for (int i = 0; i < departures.Count; i++)
            {
                Journey? journey = BuildJourney(departures[i], allStops[i], fromCode, toCode);
                if (journey is not null)
                {
                    journeys.Add(journey);
                }
            }
            journeys.Sort();
            return journeys;
        }

        // A train only works for the trip if it stops at the destination after the departure station
        public static Journey? BuildJourney(Departure departure, List<TrainStop> stops, string fromCode, string toCode)
        {
            TrainStop? from = stops.Find(s => s.LocationCode == fromCode);
            TrainStop? to = stops.Find(s => s.LocationCode == toCode && s.IsStop);

            if (from is null || to is null || to.Order <= from.Order)
            {
                return null;
            }

            return new Journey
            {
                Departure = departure,
                ArrivalTime = to.ExpectedArrival
            };
        }

        public static List<Station> ParseStations(string xml)
        {
            var stations = new List<Station>();
            foreach (XElement item in XDocument.Parse(xml).Descendants(ns + "objStation"))
            {
                stations.Add(new Station
                {
                    Code = Text(item, "StationCode"),
                    Name = Text(item, "StationDesc"),
                    Latitude = Coordinate(item, "StationLatitude"),
                    Longitude = Coordinate(item, "StationLongitude")
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
                if (Text(item, "Locationtype") == "D")
                {
                    continue;
                }

                departures.Add(new Departure
                {
                    TrainCode = Text(item, "Traincode"),
                    TrainType = Text(item, "Traintype"),
                    TrainDate = Text(item, "Traindate"),
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

        public static List<TrainStop> ParseTrainStops(string xml)
        {
            var stops = new List<TrainStop>();
            foreach (XElement item in XDocument.Parse(xml).Descendants(ns + "objTrainMovements"))
            {
                stops.Add(new TrainStop
                {
                    LocationCode = Text(item, "LocationCode"),
                    LocationName = Text(item, "LocationFullName"),
                    LocationType = Text(item, "LocationType"),
                    Order = Number(item, "LocationOrder"),
                    ExpectedArrival = ShortTime(Text(item, "ExpectedArrival"))
                });
            }
            stops.Sort((a, b) => a.Order.CompareTo(b.Order));
            return stops;
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

        // The API pads some values with spaces
        private static string Text(XElement item, string name)
        {
            return item.Element(ns + name)?.Value.Trim() ?? string.Empty;
        }

        private static int Number(XElement item, string name)
        {
            return int.TryParse(Text(item, name), out int value) ? value : 0;
        }

        // The API always uses "54.0009"; InvariantCulture stops a pt-BR machine reading it as 540009
        private static double Coordinate(XElement item, string name)
        {
            return double.TryParse(Text(item, name), NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ? value : 0;
        }

        private static string ShortTime(string time)
        {
            return time.Length >= 5 ? time.Substring(0, 5) : time;
        }
    }
}
