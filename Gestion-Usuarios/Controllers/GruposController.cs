using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Controllers
{
    public class GruposController : Controller
    {
        public IActionResult Index()
        {
            // TODO: obtener lista de grupos
            return View();
        }
    }
}