using Amazon.Lambda.Core;
using System.Text.Json.Nodes;
using FlightLib;
using DBAccessLib;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FlightRetriever;

public class Function
{
    // Main to allow local debugging
    // Add `<OutputType>Exe</OutputType>` to the <PropertyGroup>
    // Run with `dotnet run --project FlightRetriever.csproj`
    public static async Task Main(string[] args)
    {
        await DoWork();
    }

    // The cloud deployment uses this function
    // Create zip for deployment using `dotnet lambda package`
    public async Task<string> FunctionHandler(string input, ILambdaContext context)
    {
        return await DoWork();
    }

    static async Task<string> DoWork()
    {
        JsonNode api_resp = JsonNode.Parse(await GetAllPlanes())!;
        JsonNode states = api_resp["states"]!;

        long runtime_ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dbw = new DBAccess();
        var flights = new List<Flight>();

        // Each state is an array of values
        foreach (var state in states.AsArray()!)
        {
            // If we don't know either the lat or long then don't add this flight
            if (state![4] == null || state![5] == null) { continue; }

            flights.Add(new Flight(state!));
        }

        dbw.WriteFlightsToDB(flights, runtime_ts);

        // Save matches to DB
        var search_zone_resp = JsonNode.Parse(await RequestSearchZones(runtime_ts))!;
        if (search_zone_resp["errMsg"]!.ToString() != "")
        {
            Console.WriteLine($"Zone searcher service hit error: {search_zone_resp["errMsg"]!}");
        }
        else
        {
            Console.WriteLine($"Called zone searcher service. Found {search_zone_resp["matchesFound"]!} matches");
        }

        // Send notifications for found matches
        var notification_resp = JsonNode.Parse(await SendNotifications(runtime_ts))!;
        if (notification_resp["errMsg"]!.ToString() != "")
        {
            Console.WriteLine($"Notification service hit error: {notification_resp["errMsg"]!}");
        }
        else
        {
            Console.WriteLine($"Called notification service. Sent {notification_resp["emailsSent"]!} notifications");
        }

        // AWS Lambda functions are typically designed to have output, but our use case has no useful output
        // as it is not an API endpoint and is instead run on a schedule. Returning an empty string since
        // we have to return something
        return "";
    }

    static async Task<string> MakeRequestGetStr(string _base, string uri)
    {
        try
        {
            HttpClient httpClient = new(){ BaseAddress = new Uri(_base) };
            using HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error when making request to {_base}/{uri}: {ex.Message}");
            Environment.Exit(1);
            return ""; // Will never get here, keep the compiler happy
        }
    }

    static async Task<string> GetAllPlanes()
    {
        return await MakeRequestGetStr("https://opensky-network.org", "api/states/all");
    }

    static async Task<string> RequestSearchZones(long timestamp)
    {
        var zone_searcher_uri = Environment.GetEnvironmentVariable("ZONE_SEARCHER_URI");
        if (!string.IsNullOrEmpty(zone_searcher_uri))
        {
            return await MakeRequestGetStr(zone_searcher_uri, $"timestamp?ts={timestamp}");
        }

        return "{\"errMsg\" : \"Env var ZONE_SEARCHER_URI does not exist\"}";
    }

    static async Task<string> SendNotifications(long timestamp)
    {
        var notification_uri = Environment.GetEnvironmentVariable("NOTIFICATION_URI");
        if (!string.IsNullOrEmpty(notification_uri))
        {
            return await MakeRequestGetStr(notification_uri, $"notification?ts={timestamp}");
        }

        return "{\"errMsg\" : \"Env var NOTIFICATION_URI does not exist\"}";
    }
}
