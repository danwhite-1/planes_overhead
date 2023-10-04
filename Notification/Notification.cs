using DBAccessLib;
using FlightLib;
using ResponseJsonLib;

namespace Notification;

public static class Notifier
{
    //! TODO: Return json
    public static string DoNotifications(string timestamp)
    {
        var validInput = long.TryParse(timestamp, out long timestamp_l);
        if (!validInput) { return GetResponseStr.ConstructErrResp(400, "The provided ts is not valid."); }

        var db = new DBAccess();
        List<(int, int, int)> zonematchinfo = db.GetZoneMatchInfoForTimestamp(timestamp_l);

        var userFlightMap = new Dictionary<int, List<int>>();
        foreach (var zonematchi in zonematchinfo)
        {
            if (!userFlightMap.ContainsKey(zonematchi.Item1))
            {
                userFlightMap[zonematchi.Item1] = new List<int>{zonematchi.Item3};
            }
            else
            {
                userFlightMap[zonematchi.Item1].Add(zonematchi.Item3);
            }
        }

        foreach (var user in userFlightMap)
        {
            var flightInfo = db.GetFlightsByIds(user.Value);
            var addr = db.GetEmailAddressByUserId(user.Key);
            if (string.IsNullOrEmpty(addr))
            {
                Console.WriteLine($"Found empty email address for user: {user.Key}");
                continue;
            }

            var email = new Email(addr, "Planes Overhead Report", ConstructEmailString(flightInfo, timestamp_l));
            SendEmail(email);
        }

        // TODO: Update response class to version agnostic/innterface
        return GetResponseStr.ConstructSuccessResp(0, 0);
    }

    public static string ConstructEmailString(List<Flight> flightInfo, long timestamp)
    {
        DateTime time = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        string text = $"The following flights where detected in your search zone at {time.TimeOfDay} on {time.ToShortDateString()}:";

        foreach (var flight in flightInfo)
        {
            text += $"{Environment.NewLine}{flight.Callsign}\t{flight.Velocity}kts @ {flight.Geo_altitude}m";
        }
        return text;
    }

    public static void SendEmail(Email e)
    {
        Console.WriteLine($"To: {e.Address}{Environment.NewLine}Subject: {e.Subject}{Environment.NewLine}{Environment.NewLine}{e.Text}");
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