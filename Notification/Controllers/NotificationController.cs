using Microsoft.AspNetCore.Mvc;

namespace Notification.Controllers;

[ApiController]
[Route("notification")]
public class NotificationController : ControllerBase
{
    [HttpGet]
    public ActionResult Index(string ts)
    {
        string resp = Notifier.DoNotifications(ts);
        return Content(resp);
    }
}
