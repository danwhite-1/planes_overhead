using Microsoft.AspNetCore.Mvc;

namespace zone_searcher.Controllers;

[ApiController]
[Route("timestamp")]
public class ZoneSearchController : ControllerBase
{
    [HttpGet]
    public ActionResult Index(string ts)
    {
        var res = ZoneSearcher.SearchZones(ts);
        return Content(res, "application/json");
    }
}
