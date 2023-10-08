using System.Device.Location;
using DBAccessLib;
using SearchZoneLib;

namespace zone_searcher;

public static class ZoneSearcher
{
    public static string SearchZones(string timestamp)
    {
        var validInput = long.TryParse(timestamp, out long timestamp_l);
        if (!validInput) { return new ZoneSearchResponse(400, "The provided ts is not valid.").ToJsonStr(); }

        DBAccess Db = new();

        var flights = Db.GetAllFlightsForTimestamp(timestamp_l);
        if (flights.Count == 0) { return new ZoneSearchResponse(400, "No flights found for provided timestamp.").ToJsonStr(); }

        var zones = Db.GetAllSearchZones();

        List<ZoneMatch> matches = new();
        foreach (var flight in flights)
        {
            foreach (var zone in zones)
            {
                GeoCoordinate flight_pos = new (flight.Latitude, flight.Longitude);
                var distance = zone.Point.GetDistanceTo(flight_pos);
                if (distance <= zone.Distance)
                {
                    matches.Add(new ZoneMatch(flight, zone));
                }
            }
        }

        Db.WriteZoneMatchesToDB(matches);
        return new ZoneSearchResponse(matches.Count, flights.Count).ToJsonStr();
    }
}
