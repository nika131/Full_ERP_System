using Microsoft.AspNetCore.Mvc;

namespace NexusERP.Api.Controllers
{
    public class PosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
