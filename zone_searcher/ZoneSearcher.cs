using System.Device.Location;
using Newtonsoft.Json;
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
        var validInput = long.TryParse(Timestamp, out long timestamp_l);
        if (!validInput) { return ConstructErrResp(400, "The provided ts is not valid."); }

        Db = new DBAccess();
        var flights = Db.GetAllFlightsForTimestamp(timestamp_l);
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
        Console.WriteLine($"Number of matches found: {matches.Count}");
        return ConstructSuccessResp(matches.Count, flights.Count);
    }

    public string ConstructErrResp(int code, string errMsg)
    {
        Response r = new Response(code, errMsg);
        return r.toJsonStr();
    }

    public string ConstructSuccessResp(int matchesFound, int flightsChecked)
    {
        Response r = new Response(200, matchesFound, flightsChecked);
        return r.toJsonStr();
    }

    public DBAccess? Db { get; set; }
    public string Timestamp { get; set; }
}

public struct Response
{
    public Response(int _code, string _errMsg)
    {
        code = _code;
        errMsg = _errMsg;
    }

    public Response(int _code, int _matchesFound, int _flightsChecked)
    {
        code = _code;
        matchesFound = _matchesFound;;
        flightsChecked = _flightsChecked;
    }

    public string toJsonStr()
    {
        return JsonConvert.SerializeObject(this);
    }

    public int code;
    public int matchesFound;
    public int flightsChecked;
    public string errMsg;
}
