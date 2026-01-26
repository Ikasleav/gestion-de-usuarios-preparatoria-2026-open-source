using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Controllers
{
    public class UsuariosController : Controller
    {
        public IActionResult Index()
        {
            // TODO: sustituir por datos reales desde la capa de servicio / BD
            return View();
        }
    }
}