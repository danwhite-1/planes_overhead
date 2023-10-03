using DBAccessLib;
using FlightLib;
using SearchZoneLib;

namespace Notification;

public static class Notifier
{
    //! TODO: Return json
    public static string DoNotifications(string timestamp)
    {
        var validInput = long.TryParse(timestamp, out long timestamp_l); // TODO: return error if false here
        var db = new DBAccess();
        List<(int, int, int)> zonematchinfo = db.GetZoneMatchInfoForTimestamp(timestamp_l);
        
        var userFlightMap = new Dictionary<int, List<int>>();
        foreach (var zonematchi in zonematchinfo)
        {
            if (!userFlightMap.ContainsKey(zonematchi.Item1))
            {
                userFlightMap.Add(zonematchi.Item1, new List<int>(zonematchi.Item3));
            }
            else
            {
                userFlightMap[zonematchi.Item1].Add(zonematchi.Item3);
            }
        }

        foreach (var user in userFlightMap)
        {
            var flightInfo = db.GetFlightsByIds(user.Value);
            Console.WriteLine(ConstructEmailString(flightInfo, timestamp));
        }

        return "SUCCESS";
    }

    public static string ConstructEmailString(List<Flight> flightInfo, string timestamp)
    {
        string text = $"The following flights where detected in your search zone at {timestamp}";
        foreach (var flight in flightInfo)
        {
            text = text + $"{Environment.NewLine}{flight.Callsign}\t{flight.Velocity}kts @ {flight.Geo_altitude}m";
        }
        return text;
    }
}