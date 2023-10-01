using MySqlConnector;
using FlightLib;
using SearchZoneLib;

namespace DBAccessLib;

public class DBAccess
{
    //! Constructor
    //! Will throw if relevant env vars are not set
    public DBAccess()
    {
        var envVars = GetEnvVars();
        var builder = new MySqlConnectionStringBuilder
        {
            Server = envVars["HOST"],
            UserID = envVars["USER"],
            Password = envVars["PW"],
            Database = envVars["DB"],
        };

        conn = new MySqlConnection(builder.ConnectionString);
        conn.Open();

        VerifyTablesExist();
    }

    //! Get env variables and throw if not all available
    public static Dictionary<string, string> GetEnvVars()
    {
        var env_vars = new Dictionary<string, string>{
            {"HOST", Environment.GetEnvironmentVariable("HOST")!},
            {"USER", Environment.GetEnvironmentVariable("USER")!},
            {"PW", Environment.GetEnvironmentVariable("PW")!},
            {"DB", Environment.GetEnvironmentVariable("DB")!},
        };

        foreach(KeyValuePair<string, string> env_var in env_vars)
        {
            if (string.IsNullOrEmpty(env_var.Value))
            {
                throw new Exception($"Env variable {env_var.Key} is not set");
            }
        }

        return env_vars;
    }

    ///////////////////////////
    //! Create Table Functions
    ///////////////////////////
    public void CreateFlightTable()
    {
        string t_name = "flights";
        string t_schema = @"
`flightid` INT NOT NULL AUTO_INCREMENT,
`query_timestamp` BIGINT,
`transponderid` VARCHAR(6),
`callsign` VARCHAR(10),
`origin_country` VARCHAR(60),
`timestamp` BIGINT,
`last_contact` BIGINT,
`latitude` DOUBLE,
`longitude` DOUBLE,
`baro_altitude` FLOAT,
`on_ground` BOOLEAN,
`velocity` FLOAT,
`true_track` FLOAT,
`vertical_rate` FLOAT,
`sensors` VARCHAR(255),
`geo_altitude` FLOAT,
`squak` VARCHAR(4),
`spi` BOOLEAN,
`position_source` INT,
`category` INT,
PRIMARY KEY (flightid)";

        CreateTable(t_name, t_schema);
    }

    public void CreateZoneMatchTable()
    {
        string t_name = "zonematches";
        string t_schema = @"
`zonematchid` INT NOT NULL AUTO_INCREMENT,
`flightid` INT NOT NULL,
`zoneid` INT NOT NULL,
PRIMARY KEY (zonematchid)";

        CreateTable(t_name, t_schema);
    }

    public void CreateUserTable()
    {
        string t_name = "users";
        string t_schema = @"
`userid` INT NOT NULL AUTO_INCREMENT,
`email` VARCHAR(320),
PRIMARY KEY (userid)";

        CreateTable(t_name, t_schema);
    }

    public void CreateSearchZonesTable()
    {
        string t_name = "searchzones";
        string t_schema = @"
`searchzoneid` INT NOT NULL AUTO_INCREMENT,
`point` VARCHAR(40) NOT NULL,
`distance` INT NOT NULL,
`userid` INT NOT NULL,
PRIMARY KEY (searchzoneid),
FOREIGN KEY (userid) REFERENCES users(userid)";

        CreateTable(t_name, t_schema);
    }

    public void CreateTable(string name, string schema)
    {
        string createTable_sql = @$"CREATE TABLE IF NOT EXISTS `{name}` ({schema});";

        var cmd = new MySqlCommand(createTable_sql, conn);
        cmd.ExecuteNonQuery();

        string checkExists_sql = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name='{name}'";
        cmd = new MySqlCommand(checkExists_sql, conn);
        var rows = cmd.ExecuteScalar();

        if (Convert.ToInt16(rows) != 1)
        {
            throw new Exception($"{name} table not able to be created");
        }
    }

    ////////////////////
    //! Write Functions
    ////////////////////
    public void WriteFlightsToDB(List<Flight> flights, long timestamp)
    {
        if (flights.Count == 0) { return; }

        string insert_sql = $"INSERT INTO flights(query_timestamp, transponderid, callsign, origin_country, timestamp, last_contact, latitude, longitude, baro_altitude, on_ground, velocity, true_track, vertical_rate, sensors, geo_altitude, squak, spi, position_source, category) VALUES";
        var cmd = new MySqlCommand(insert_sql, conn);

        int count = 0;
        foreach (var flight in flights) 
        {
            string values_sql = $"({timestamp}, @transponderid{count}, @callsign{count}, @origin_country{count}, @timestamp{count}, @last_contact{count}, @latitude{count}, @longitude{count}, @baro_altitude{count}, @on_ground{count}, @velocity{count}, @true_track{count}, @vertical_rate{count}, @sensors{count}, @geo_altitude{count}, @squak{count}, @spi{count}, @position_source{count}, @category{count}),";
            cmd.CommandText += values_sql;
            cmd.Parameters.AddWithValue($"@transponderid{count}", flight.TransponderId);
            cmd.Parameters.AddWithValue($"@callsign{count}", flight.Callsign);
            cmd.Parameters.AddWithValue($"@origin_country{count}", flight.Origin_country);
            cmd.Parameters.AddWithValue($"@timestamp{count}", flight.Timestamp);
            cmd.Parameters.AddWithValue($"@last_contact{count}", flight.Last_contact);
            cmd.Parameters.AddWithValue($"@latitude{count}", flight.Latitude);
            cmd.Parameters.AddWithValue($"@longitude{count}", flight.Longitude);
            cmd.Parameters.AddWithValue($"@baro_altitude{count}", flight.Baro_altitude);
            cmd.Parameters.AddWithValue($"@on_ground{count}", flight.On_ground);
            cmd.Parameters.AddWithValue($"@velocity{count}", flight.Velocity);
            cmd.Parameters.AddWithValue($"@true_track{count}", flight.True_track);
            cmd.Parameters.AddWithValue($"@vertical_rate{count}", flight.Vertical_rate);
            cmd.Parameters.AddWithValue($"@sensors{count}", flight.Sensors);
            cmd.Parameters.AddWithValue($"@geo_altitude{count}", flight.Geo_altitude);
            cmd.Parameters.AddWithValue($"@squak{count}", flight.Squak);
            cmd.Parameters.AddWithValue($"@spi{count}", flight.Spi);
            cmd.Parameters.AddWithValue($"@position_source{count}", flight.Position_source);
            cmd.Parameters.AddWithValue($"@category{count}", flight.Category);
            count++;
        }
        cmd.CommandText = cmd.CommandText.TrimEnd(',');
        var rows = cmd.ExecuteNonQuery();
        Console.WriteLine($"{rows} written to db. Number of flights: {flights.Count}");
    }

    public void WriteZoneMatchesToDB(List<ZoneMatch> zonematches)
    {
        if (zonematches.Count == 0) { return; }

        string insert_sql = $"INSERT INTO zonematches(flightid, zoneid) VALUES";
        var cmd = new MySqlCommand(insert_sql, conn);

        int count = 0;
        foreach (var zonematch in zonematches) 
        {
            string values_sql = $"(@flightid{count}, @zoneid{count}),";
            cmd.CommandText += values_sql;
            cmd.Parameters.AddWithValue($"@flightid{count}", zonematch.flight.Id);
            cmd.Parameters.AddWithValue($"@zoneid{count}", zonematch.searchzone.Id);
            count++;
        }
        cmd.CommandText = cmd.CommandText.TrimEnd(',');
        var rows = cmd.ExecuteNonQuery();
        Console.WriteLine($"{rows} written to db. Number of zone matches: {zonematches.Count}");
    }

    /////////////////////
    //! Select Functions
    /////////////////////
    public List<Flight> GetAllFlightsForTimestamp(long ts)
    {
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT flightid, transponderid, callsign, latitude, longitude, baro_altitude, on_ground, velocity, vertical_rate, geo_altitude, squak FROM flights WHERE query_timestamp=@ts";
        cmd.Parameters.AddWithValue("@ts",ts);

        List<Flight> flights = new();
        MySqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            try {
                var id = getValueFromReaderNullSafe<int>(reader, 0);
                var transponderId = getValueFromReaderNullSafeStr(reader, 1);
                var callsign = getValueFromReaderNullSafeStr(reader, 2);
                var latitude = getValueFromReaderNullSafe<double>(reader, 3);
                var longitude =  getValueFromReaderNullSafe<double>(reader, 4);
                var baro_altitude = getValueFromReaderNullSafe<float>(reader, 5);
                var on_ground = getValueFromReaderNullSafe<bool>(reader, 6);
                var velocity = getValueFromReaderNullSafe<float>(reader, 7);
                var vertical_rate = getValueFromReaderNullSafe<float>(reader, 8);
                var geo_altitude = getValueFromReaderNullSafe<float>(reader, 9);
                var squak = getValueFromReaderNullSafeStr(reader, 10);
                flights.Add(new Flight(id, transponderId, callsign, latitude, longitude, baro_altitude, on_ground, velocity, vertical_rate, geo_altitude, squak));
            }
            catch (FormatException fe) {
                Console.WriteLine($"Format exception encountered: {fe.Message}");
            }
            catch (InvalidOperationException ioe) {
                Console.WriteLine($"Invalid operation exception encountered: {ioe.Message}");
            }
        }
        reader.Close();

        return flights;
    }

    public List<SearchZone> GetAllSearchZones()
    {
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM searchzones;";

        List<SearchZone> sz = new();
        MySqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            try {
                var id = getValueFromReaderNullSafe<int>(reader, 0);
                var point = getValueFromReaderNullSafeStr(reader, 1);
                var distance = getValueFromReaderNullSafe<int>(reader, 2);
                sz.Add(new SearchZone(id, point, distance));
            }
            catch (FormatException fe) {
                Console.WriteLine($"Format exception encountered: {fe.Message}");
            }
            catch (InvalidOperationException ioe) {
                Console.WriteLine($"Invalid operation exception encountered: {ioe.Message}");
            }
        }
        reader.Close();

        return sz;
    }

    //////////////////////
    //! Utility Functions
    //////////////////////
    public void VerifyTablesExist()
    {
        var tableMap = new Dictionary<string, Action>()
        {
            { "flights", CreateFlightTable },
            { "zonematches", CreateZoneMatchTable },
            { "users", CreateUserTable },
            { "searchzones", CreateSearchZonesTable },
        };

        foreach(KeyValuePair<string, Action> table in tableMap)
        {
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name='{table.Key}'";;
            cmd.Parameters.AddWithValue("@table", table.Key);
            cmd.Prepare();

            if (Convert.ToInt16(cmd.ExecuteScalar()) != 0) { continue; }

            try
            {
                table.Value();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    // No default contructor for a string, we need a separate function for it
    public string getValueFromReaderNullSafeStr(MySqlDataReader reader, int idx)
    {
        return reader.GetValue(idx) != DBNull.Value ? reader.GetString(idx) : "";
    }

    // We need to deal with lat/long better than this... 0.0 could be a valid value
    public T getValueFromReaderNullSafe<T>(MySqlDataReader reader, int idx) where T : new()
    {
        return reader.GetValue(idx) != DBNull.Value ? reader.GetFieldValue<T>(idx) : new T();
    }

    private readonly MySqlConnection conn;
}
