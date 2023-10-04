using Newtonsoft.Json;

namespace ResponseJsonLib;

public class Response
{
    public Response(int _code, string _errMsg)
    {
        code = _code;
        errMsg = _errMsg;
    }

    public Response(int _code, int _matchesFound, int _flightsChecked)
    {
        code = _code;
        matchesFound = _matchesFound;;
        flightsChecked = _flightsChecked;
    }

    public string ToJsonStr()
    {
        return JsonConvert.SerializeObject(this);
    }

    public int code;
    public int matchesFound;
    public int flightsChecked;
    public string errMsg = "";
}
