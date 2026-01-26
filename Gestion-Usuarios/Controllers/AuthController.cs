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
            // Aquí iría tu lógica real de validación de usuario
            // Por ahora, simulamos un login exitoso redirigiendo al Home

            if (true) // Simulación
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // Acción para el botón "Salir"
        public IActionResult Logout()
        {
            // Aquí iría la lógica para limpiar cookies/sesión
            return RedirectToAction("Login");
        }
    }
}