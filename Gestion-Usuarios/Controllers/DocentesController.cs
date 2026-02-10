using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Controllers
{
    [Authorize]
    public class DocentesController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Dashboard/Docentes/Index.cshtml");
        }
    }
}