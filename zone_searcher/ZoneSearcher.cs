using System.Device.Location;
using DBAccessLib;
using SearchZoneLib;

namespace zone_searcher;

public class ZoneSearcher
{
    public ZoneSearcher(string ts)
    {
        Timestamp = ts;
    }

    public string SearchZones()
    {
        long ts_l;
        try {
            ts_l = long.Parse(Timestamp);
        }
        catch (Exception) {
            return "ERROR";
        }

        Db = new DBAccess();
        var flights = Db.GetAllFlightsForTimestamp(ts_l);
        var zones = Db.GetAllSearchZones();

        List<ZoneMatch> matches = new();
        foreach (var flight in flights)
        {
            foreach (var zone in zones)
            {
                GeoCoordinate flight_pos = new GeoCoordinate(flight.Latitude, flight.Longitude);
                var distance = zone.Point.GetDistanceTo(flight_pos);
                if (distance <= zone.Distance)
                {
                    matches.Add(new ZoneMatch(flight, zone));
                }
            }
        }

        Db.WriteZoneMatchesToDB(matches);

        return "SUCCESS";
    }

    public DBAccess? Db { get; set; }
    public string Timestamp { get; set; }
}
