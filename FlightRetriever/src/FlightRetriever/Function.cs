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
    // Create zip for deployment using `dotnet lambda deploy-function FlightRetriever --region eu-north-1`
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
        var search_zone_resp = JsonNode.Parse(await RequestSearchZones(runtime_ts))!;

        if (search_zone_resp["errMsg"]!.ToString() != "")
        {
            Console.WriteLine($"Zone searcher service hit error: {search_zone_resp["errMsg"]!}");
        }
        else
        {
            Console.WriteLine($"Called zone searcher service. Found {search_zone_resp["matchesFound"]!} matches");
        }

        // TODO: Make this function report success or failure
        return "{ statusCode : 200 }";
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
        return await MakeRequestGetStr("http://localhost:5000", $"timestamp?ts={timestamp}");
    }
}
