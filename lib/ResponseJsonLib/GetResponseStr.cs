namespace ResponseJsonLib;

public static class GetResponseStr
{
    public static string ConstructErrResp(int code, string errMsg)
    {
        Response r = new(code, errMsg);
        return r.ToJsonStr();
    }

    public static string ConstructSuccessResp(int matchesFound, int flightsChecked)
    {
        Response r = new(200, matchesFound, flightsChecked);
        return r.ToJsonStr();
    }
}