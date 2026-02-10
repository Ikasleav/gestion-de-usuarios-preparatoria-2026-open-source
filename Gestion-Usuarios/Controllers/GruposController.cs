using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Controllers
{
    public class GruposController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Dashboard/Grupos/Index.cshtml");
        }
    }
}