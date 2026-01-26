using Microsoft.AspNetCore.Mvc;

namespace ChecklistSystems.Controllers
{
    public class HistoricosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}