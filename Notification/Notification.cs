using DBAccessLib;
using FlightLib;

namespace Notification;

public static class Notifier
{
    public static string DoNotifications(string timestamp)
    {
        var validInput = long.TryParse(timestamp, out long timestamp_l);
        if (!validInput) { return new NotificationResponse(400, "The provided ts is not valid.").ToJsonStr(); }

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

        var emails = new List<Email>();
        foreach (var user in userFlightMap)
        {
            var flightInfo = db.GetFlightsByIds(user.Value);
            var addr = db.GetEmailAddressByUserId(user.Key);
            if (string.IsNullOrEmpty(addr))
            {
                Console.WriteLine($"Found empty email address for user: {user.Key}");
                continue;
            }

            emails.Add(new Email(addr, "Planes Overhead Report", ConstructEmailString(flightInfo, timestamp_l)));
        }

        SendEmails(emails);

        return new NotificationResponse(emails.Count).ToJsonStr();
    }

    public static string ConstructEmailString(List<Flight> flightInfo, long timestamp)
    {
        DateTime time = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        string text = $"The following flights where detected in your search zone at {time.TimeOfDay} on {time.ToShortDateString()}:";

        text += @$"{Environment.NewLine}<table style='border: 1px solid black;'>
<tr style='border: 1px solid black;'>
    <th style='border: 1px solid black; width: 80px;'>Callsign</th>
    <th style='border: 1px solid black; width: 135px'>Ground Speed (kts)</th>
    <th style='border: 1px solid black; width: 80px'>Altitude (m)</th>
</tr>";

        foreach (var flight in flightInfo)
        {
            text += @$"
<tr style='border: 1px solid black;'>
    <td style='border: 1px solid black;'>{flight.Callsign}</td>
    <td style='border: 1px solid black;' align='right'>{flight.Velocity}</td>
    <td style='border: 1px solid black;' align='right'>{flight.Geo_altitude}</td>
</tr>";
        }

        text += "</table>";
        return text;
    }

    public static void SendEmails(List<Email> emails)
    {
        var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
        var senderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL");
        var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PW");
        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(emailPassword))
        {
            throw new Exception("Not all environment vars set for SMTP");
        }

        var emailSender = new EmailSender(smtpServer, senderEmail, emailPassword);
        emailSender.SendEmails(emails);
    }
}
