using MySqlConnector;
using System.Configuration;

namespace PlanesOverhead
{
    public class DBWriter
    {
        public DBWriter(long ts)
        {
            // TODO make these be read externally
            var builder = new MySqlConnectionStringBuilder
            {
                Server = ConfigurationManager.AppSettings["host"],
                UserID = ConfigurationManager.AppSettings["user"],
                Password = ConfigurationManager.AppSettings["password"],
                Database = ConfigurationManager.AppSettings["database"],
            };

            conn = new MySqlConnection(builder.ConnectionString);
            conn.Open();

            try
            {
                CreateFlightTable(ts);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void WriteFlightsToDB(List<Flight> flights, long timestamp)
        {
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

        public void CreateFlightTable(long timestamp)
        {
            string createTable_sql = @$"CREATE TABLE IF NOT EXISTS `flights`
(
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
PRIMARY KEY (flightid)
);";

            var cmd = new MySqlCommand(createTable_sql, conn);
            cmd.Parameters.AddWithValue("@timestamp", timestamp);
            cmd.ExecuteNonQuery();

            string checkExists_sql = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name='flights'";
            cmd = new MySqlCommand(checkExists_sql, conn);
            var rows = cmd.ExecuteScalar();

            if (Convert.ToInt16(rows) != 1)
            {
                throw new Exception("Flights table not able to be created");
            }
        }

        private readonly MySqlConnection conn;
    }
}