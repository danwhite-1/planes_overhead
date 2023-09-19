using Amazon.Lambda.Core;
using System.Text.Json.Nodes;
using FlightLib;
using DBAccessLib;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FlightRetriever;

public class Function
{
    public async Task<string> FunctionHandler(string input, ILambdaContext context)
    {
        JsonNode api_resp = JsonNode.Parse(await GetAllPlanes())!;
        JsonNode states = api_resp["states"]!;

        long runtime_ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dbw = new DBAccess();
        dbw.CreateFlightTable(runtime_ts); // DANIEL - fix this later
        var flights = new List<Flight>();

        foreach (var state in states.AsArray()!)
        {
            flights.Add(new Flight(state!));
        }

        dbw.WriteFlightsToDB(flights, runtime_ts);
        RequestSearchZones(runtime_ts);

        // TODO: Make this function report success or failure
        return "{ statusCode : 200 }";
    }

    static async Task<string> GetAllPlanes()
    {
        HttpClient httpClient = new(){ BaseAddress = new Uri("https://opensky-network.org") };
        using HttpResponseMessage response = await httpClient.GetAsync("api/states/all");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    // Stub function, yet to be implemented
    static void RequestSearchZones(long timestamp)
    {
        Console.WriteLine($"Making request to zone search server for {timestamp}... Functionality not yet implemented");
    }
}
