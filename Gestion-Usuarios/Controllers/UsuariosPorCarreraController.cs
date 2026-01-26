using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Controllers
{
    public class UsuariosPorCarreraController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}