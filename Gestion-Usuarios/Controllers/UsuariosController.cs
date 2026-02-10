using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Dashboard/Usuarios/Index.cshtml");
        }
    }
}