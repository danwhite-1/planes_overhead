using System.Text.Json.Nodes;

namespace PlanesOverhead
{
    class Program
    {
        static async Task Main()
        {
            JsonNode api_resp = JsonNode.Parse(await GetAllPlanes())!;
            JsonNode states = api_resp["states"]!;

            long runtime_ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var dbw = new DBWriter(runtime_ts);
            var flights = new List<Flight>();

            foreach (var state in states.AsArray()!)
            {
                flights.Add(new Flight(state!));
            }

            dbw.WriteFlightsToDB(flights, runtime_ts);
            RequestSearchZones(runtime_ts);
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
}