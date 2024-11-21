using Microsoft.AspNetCore.Mvc;

namespace FodmapLog.Server.Controllers
{
    public class FodmapLogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
