using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Controllers
{
    public class DocentesController : Controller
    {
        public IActionResult Index()
        {
            // TODO: obtener lista de docentes
            return View();
        }
    }
}