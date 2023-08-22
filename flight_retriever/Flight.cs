using System.Text.Json.Nodes;

namespace FlightRetriever
{
    public class Flight
    {
        public Flight(JsonNode state)
        {
            try {
                TransponderId = state[0]?.GetValue<string>();
                Callsign = state[1]?.GetValue<string>();
                Origin_country = state[2]?.GetValue<string>();
                Timestamp = state[3]?.GetValue<int>();
                Last_contact = state[4]?.GetValue<int>();
                Latitude = state[5]?.GetValue<float>();
                Longitude = state[6]?.GetValue<float>();
                Baro_altitude = state[7]?.GetValue<float>();
                On_ground = state[8]?.GetValue<bool>();
                Velocity = state[9]?.GetValue<float>();
                True_track = state[10]?.GetValue<float>();
                Vertical_rate = state[11]?.GetValue<float>();
                Sensors = state[12]?.GetValue<int[]>();
                Geo_altitude = state[13]?.GetValue<float>();
                Squak = state[14]?.GetValue<string>();
                Spi = state[15]?.GetValue<bool>();
                Position_source = state[16]?.GetValue<int>();
            }
            catch (FormatException fe) {
                Console.WriteLine($"Format exception encountered: {fe.Message}");
            }
            catch (InvalidOperationException ioe) {
                Console.WriteLine($"Invalid operation exception encountered: {ioe.Message}");
            }
        }

        public string? TransponderId { get; private set; }
        public string? Callsign { get; private set; }
        public string? Origin_country { get; private set; }
        public int? Timestamp { get; private set; }
        public int? Last_contact { get; private set; }
        public float? Latitude { get; private set; }
        public float? Longitude { get; private set; }
        public float? Baro_altitude { get; private set; }
        public bool? On_ground { get; private set; }
        public float? Velocity { get; private set; }
        public float? True_track { get; private set; }
        public float? Vertical_rate { get; private set; }
        public int[]? Sensors { get; private set; }
        public float? Geo_altitude { get; private set; }
        public string? Squak { get; private set; }
        public bool? Spi { get; private set; }
        public int? Position_source { get; private set; }
        public int? Category { get; private set; }

        #if DEBUG
        private static string MemberToString<T>(string varName, T varVal)
        {
            return $"\t{varName} = {varVal}{Environment.NewLine}";
        }

        public override string ToString()
        {
            return string.Format($"Flight object: {Environment.NewLine}" +
                                 MemberToString<string>(nameof(TransponderId), TransponderId!) +
                                 MemberToString<string>(nameof(Callsign), Callsign!) +
                                 MemberToString<string>(nameof(Origin_country), Origin_country!) +
                                 MemberToString<int?>(nameof(Timestamp), Timestamp) +
                                 MemberToString<int?>(nameof(Last_contact), Last_contact) +
                                 MemberToString<float?>(nameof(Latitude), Latitude) +
                                 MemberToString<float?>(nameof(Longitude), Longitude) +
                                 MemberToString<float?>(nameof(Baro_altitude), Baro_altitude) +
                                 MemberToString<bool?>(nameof(On_ground), On_ground) +
                                 MemberToString<float?>(nameof(Velocity), Velocity) +
                                 MemberToString<float?>(nameof(True_track), True_track) +
                                 MemberToString<float?>(nameof(Vertical_rate), Vertical_rate) +
                                 MemberToString<int[]?>(nameof(Sensors), Sensors) +
                                 MemberToString<float?>(nameof(Geo_altitude), Geo_altitude) +
                                 MemberToString<string>(nameof(Squak), Squak!) +
                                 MemberToString<bool?>(nameof(Spi), Spi) +
                                 MemberToString<int?>(nameof(Position_source), Position_source) +
                                 MemberToString<int?>(nameof(Category), Category));
        }
        #endif
    }
}