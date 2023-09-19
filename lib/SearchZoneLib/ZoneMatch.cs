using FlightLib;

namespace SearchZoneLib;

public struct ZoneMatch
{
    public ZoneMatch(Flight f, SearchZone sz)
    {
        flight = f;
        searchzone = sz;
    }
    public Flight flight;
    public SearchZone searchzone;
}
