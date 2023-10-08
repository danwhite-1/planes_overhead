using Newtonsoft.Json;

namespace ResponseJsonLib;

public class Response
{
    public Response(int _code)
    {
        code = _code;
    }

    public Response(int _code, string _errMsg)
    {
        code = _code;
        errMsg = _errMsg;
    }

    public string ToJsonStr()
    {
        return JsonConvert.SerializeObject(this);
    }

    public int code;
    public string errMsg = "";
}
