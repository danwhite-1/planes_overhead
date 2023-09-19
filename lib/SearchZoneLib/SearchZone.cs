using System.Device.Location;

namespace SearchZoneLib;

public class SearchZone
{
    public SearchZone(int id, string point, int distance)
    {
        Id = id;
        Distance = distance;

        // DANIEL - need better error handling here
        string[] point_parts = point.Split(',');
        Point = new GeoCoordinate(Convert.ToDouble(point_parts[0]), Convert.ToDouble(point_parts[1]));
    }

    public int Id { get; set; }
    public GeoCoordinate Point { get; set; }
    public int Distance { get; set; }
}
