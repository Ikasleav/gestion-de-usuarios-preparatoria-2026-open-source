using Microsoft.AspNetCore.Mvc;

namespace ChecklistSystems.Controllers
{
    public class AlumnosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}