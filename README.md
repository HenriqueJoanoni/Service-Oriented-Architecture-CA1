# Trainsportation

A Blazor Server app for train commuters in Ireland. It shows which direct trains get you from A to B, whether they're running late, and what the weather will be when you arrive.

Built for the SD4 Service Oriented Architecture CA1 at DkIT.

## Features

- **Journey planner** – pick two stations and see every direct train in the next 30/60/90 minutes, with departure, arrival, delay and the weather at your destination for the arrival hour. Sort by leaving time, arrival time or lateness.
- **Weather on arrival** – temperature, conditions and chance of rain, with a "Bring an umbrella" badge from 50%.
- **Live departures** – everything leaving a station, filterable by direction and train type, sortable by due time, lateness or destination.
- **Saved routes** – save a searched route (optionally named), open it in one click from the home page or the planner. Stored in a JSON file; A → B and B → A are separate routes.
- **Error handling** – friendly messages and Retry when an API is down or times out (10 s), input validation, and weather failures don't block the train results.
- **Responsive** – result tables turn into stacked cards on phones.

## APIs

| API | Format | Used for |
|---|---|---|
| [Irish Rail Realtime](https://api.irishrail.ie/realtime/) | XML | Stations (with coordinates), live departures, each train's stops |
| [Open-Meteo](https://open-meteo.com/en/docs) | JSON | Hourly forecast at the destination station |

No API keys needed.

## Running it

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) and an internet connection.

```bash
dotnet run --project soa-ca1
```

Then open http://localhost:5123.

Run the tests (22 xUnit tests, no network needed):

```bash
dotnet test
```

## Project structure

```text
soa-ca1/
├── Models/        Station, Departure, TrainStop, Journey, HourlyForecast, SavedRoute
├── Services/      IrishRailService, WeatherService, RouteStore
├── Components/
│   ├── Pages/     Home, JourneyPlanner, Departures
│   └── Layout/    MainLayout, NavMenu
└── wwwroot/       app.css (Bootstrap theme overrides), images
soa-ca1.Tests/
├── IrishRailParsingTests, JourneyTests, WeatherTests, SavedRouteTests
└── SampleData/    Real API responses used by the parsing tests
```

- **Services** call the APIs with RestSharp and parse the responses (`XDocument` for Irish Rail, `System.Text.Json` for Open-Meteo). Parsing methods are public so they can be tested with the saved samples.
- **Models** implement `IComparable<T>` for default sorting; `SavedRoute` overrides `Equals`/`GetHashCode` and `==`/`!=`.
- Numbers and dates from the APIs are parsed with `CultureInfo.InvariantCulture`, so the app works on machines with non-English regional settings.
- Saved routes are written to `saved-routes.json` next to the compiled app (`bin/Debug/net10.0/`).

## References

**APIs**
- Irish Rail Realtime API – https://api.irishrail.ie/realtime/
- Open-Meteo docs – https://open-meteo.com/en/docs
- Open-Meteo terms (CC BY 4.0) – https://open-meteo.com/en/terms
- Public APIs list – https://github.com/public-apis/public-apis

**Libraries and front end**
- RestSharp – https://restsharp.dev/
- Bootstrap 5.3 – https://getbootstrap.com/docs/5.3/getting-started/introduction/
- Bootstrap CSS variables – https://getbootstrap.com/docs/5.3/customize/css-variables/
- Bootstrap Icons 1.11.3 (MIT) – https://icons.getbootstrap.com/
- Work Sans font – https://fonts.google.com/specimen/Work+Sans

**.NET / C#**
- Blazor – https://learn.microsoft.com/en-us/aspnet/core/blazor/
- Blazor navigation and query strings – https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/navigation
- Hot Reload / `dotnet watch` – https://learn.microsoft.com/en-us/aspnet/core/test/hot-reload
- LINQ to XML – https://learn.microsoft.com/en-us/dotnet/standard/linq/linq-xml-overview
- System.Text.Json – https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview
- `CultureInfo.InvariantCulture` – https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.invariantculture
- `IComparable<T>` – https://learn.microsoft.com/en-us/dotnet/api/system.icomparable-1
- try-catch and exception filters – https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/try-catch
- xUnit – https://xunit.net/

Weather data by [Open-Meteo.com](https://open-meteo.com/) (CC BY 4.0).
