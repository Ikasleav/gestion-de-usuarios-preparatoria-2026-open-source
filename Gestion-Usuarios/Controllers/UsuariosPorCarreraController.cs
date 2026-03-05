using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Controllers
{
    [Authorize]
    public class UsuariosPorCarreraController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Dashboard/UsuariosPorCarrera/Index.cshtml");
        }
    }
}