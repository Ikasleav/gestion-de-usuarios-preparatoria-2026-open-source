using Microsoft.AspNetCore.Mvc;

namespace ChecklistSystems.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            

            if (true) 
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // Acción para el botón "Salir"
        public IActionResult Logout()
        {
            
            return RedirectToAction("Login");
        }
    }
}