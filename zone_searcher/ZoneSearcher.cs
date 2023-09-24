using System.Device.Location;
using Newtonsoft.Json;
using DBAccessLib;
using SearchZoneLib;

namespace zone_searcher;

public static class ZoneSearcher
{
    public static string SearchZones(string timestamp)
    {
        var validInput = long.TryParse(timestamp, out long timestamp_l);
        if (!validInput) { return ConstructErrResp(400, "The provided ts is not valid."); }

        DBAccess Db = new();

        var flights = Db.GetAllFlightsForTimestamp(timestamp_l);
        if (flights.Count == 0) { return ConstructErrResp(400, "No flights found for provided timestamp."); }

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
        return ConstructSuccessResp(matches.Count, flights.Count);
    }

    public static string ConstructErrResp(int code, string errMsg)
    {
        Response r = new(code, errMsg);
        return r.toJsonStr();
    }

    public static string ConstructSuccessResp(int matchesFound, int flightsChecked)
    {
        Response r = new(200, matchesFound, flightsChecked);
        return r.toJsonStr();
    }
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
    public string errMsg = "";
}
