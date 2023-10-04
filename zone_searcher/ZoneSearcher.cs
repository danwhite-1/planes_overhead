using System.Device.Location;
using DBAccessLib;
using SearchZoneLib;
using ResponseJsonLib;

namespace zone_searcher;

public static class ZoneSearcher
{
    public static string SearchZones(string timestamp)
    {
        var validInput = long.TryParse(timestamp, out long timestamp_l);
        if (!validInput) { return GetResponseStr.ConstructErrResp(400, "The provided ts is not valid."); }

        DBAccess Db = new();

        var flights = Db.GetAllFlightsForTimestamp(timestamp_l);
        if (flights.Count == 0) { return GetResponseStr.ConstructErrResp(400, "No flights found for provided timestamp."); }

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
        return GetResponseStr.ConstructSuccessResp(matches.Count, flights.Count);
    }
}
