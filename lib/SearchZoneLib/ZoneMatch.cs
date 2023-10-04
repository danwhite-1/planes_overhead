using FlightLib;

namespace SearchZoneLib;

public class ZoneMatch
{
    public ZoneMatch(Flight f, SearchZone sz)
    {
        flight = f;
        searchzone = sz;
    }
    public Flight flight;
    public SearchZone searchzone;
}
