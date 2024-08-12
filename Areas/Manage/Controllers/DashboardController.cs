using Microsoft.AspNetCore.Mvc;

namespace AllUp.Areas.Manage.Controllers;

[Area("Manage")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
