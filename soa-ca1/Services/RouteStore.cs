using System.Text.Json;
using soa_ca1.Models;

namespace soa_ca1.Services
{
    public class RouteStore
    {
        public static readonly string FILE_PATH = Path.Combine(AppContext.BaseDirectory, "saved-routes.json");
        private static readonly JsonSerializerOptions options = new() { WriteIndented = true };
        
        public static List<SavedRoute> Load(string path)
        {
            if (!File.Exists(path))
            {
                return new List<SavedRoute>();
            }

            string json = File.ReadAllText(path);
            List<SavedRoute> routes = JsonSerializer.Deserialize<List<SavedRoute>>(json) ?? new List<SavedRoute>();
            routes.Sort();
            return routes;
        }

        public static void Save(List<SavedRoute> routes, string path)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(routes, options));
        }

        public static bool Add(List<SavedRoute> routes, SavedRoute route)
        {
            if (string.IsNullOrWhiteSpace(route.FromCode) || string.IsNullOrWhiteSpace(route.ToCode))
            {
                throw new ArgumentException("Choose both stations before saving a route.");
            }

            if (route.FromCode == route.ToCode)
            {
                throw new ArgumentException("Choose two different stations.");
            }

            if (routes.Contains(route))
            {
                return false;
            }

            routes.Add(route);
            routes.Sort();
            return true;
        }
    }
}
