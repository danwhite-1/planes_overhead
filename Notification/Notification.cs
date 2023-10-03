using DBAccessLib;
using FlightLib;

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
            // TODO: Get actual email from db
            var email = new Email("test@test.com", "Planes Overhead Report", ConstructEmailString(flightInfo, timestamp));
            SendEmail(email);
        }

        return "SUCCESS";
    }

    public static string ConstructEmailString(List<Flight> flightInfo, string timestamp)
    {
        string text = $"The following flights where detected in your search zone at {timestamp}";
        foreach (var flight in flightInfo)
        {
            text += $"{Environment.NewLine}{flight.Callsign}\t{flight.Velocity}kts @ {flight.Geo_altitude}m";
        }
        return text;
    }

    public static void SendEmail(Email e)
    {
        // Stub function, not yet implemented
    }
}

public struct Email
{
    public Email(string addr, string sub, string tex)
    {
        Address = addr;
        Subject = sub;
        Text = tex;
    }

    public string Address;
    public string Subject;
    public string Text;
}