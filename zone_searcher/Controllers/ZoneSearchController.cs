using Microsoft.AspNetCore.Mvc;

namespace zone_searcher.Controllers;

[ApiController]
[Route("timestamp")]
public class ZoneSearchController : ControllerBase
{
    [HttpGet]
    public ActionResult Index(string ts)
    {
        var searcher = new ZoneSearcher(ts);
        var res = searcher.SearchZones();
        return Content(res);
    }
}
