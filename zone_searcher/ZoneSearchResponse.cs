using ResponseJsonLib;

namespace zone_searcher
{
    public class ZoneSearchResponse : Response
    {
        //! Error contructor
        public ZoneSearchResponse(int _code, string _errMsg) : base(_code, _errMsg) { }

        //! Success constructor
        public ZoneSearchResponse(int _matchesFound, int _flightsChecked) : base(200)
        {
            matchesFound = _matchesFound;;
            flightsChecked = _flightsChecked;
        }

        public int matchesFound;
        public int flightsChecked;
    }
}