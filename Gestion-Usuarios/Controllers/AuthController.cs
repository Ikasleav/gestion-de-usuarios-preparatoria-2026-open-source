using Gestion_Usuarios.Models;
using Gestion_Usuarios.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class AuthController : Controller
{
	private readonly ContextDb _context;

	public AuthController(ContextDb context)
	{
		_context = context;
	}

	[HttpGet]
	public IActionResult Login()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Login(LoginViewModel model)
	{
		if (!ModelState.IsValid)
			return View(model);

		var user = _context.ManagementUsers.FirstOrDefault(u =>
			u.management_user_Username == model.UserOrEmail ||
			u.management_user_Email == model.UserOrEmail);

		if (user == null)
		{
			ViewBag.Error = "Usuario o correo incorrecto";
			return View(model);
		}

		if (user.management_user_IsLocked)
		{
			ViewBag.Error = "Usuario bloqueado";
			return View(model);
		}

		if (!user.management_user_status)
		{
			ViewBag.Error = "Usuario inactivo";
			return View(model);
		}

		if (!VerifyPassword(model.Password, user.management_user_PasswordHash))
		{
			ViewBag.Error = "Contraseña incorrecta";
			return View(model);
		}

		// ✔ Login exitoso
		user.management_user_LastLoginDate = DateTime.Now;
		_context.SaveChanges();

		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, user.management_user_ID.ToString()),
			new Claim(ClaimTypes.Name, user.management_user_Username)
		};

		if (!string.IsNullOrEmpty(user.management_user_Email))
			claims.Add(new Claim(ClaimTypes.Email, user.management_user_Email));

		var identity = new ClaimsIdentity(
			claims,
			CookieAuthenticationDefaults.AuthenticationScheme);

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(identity));

		return RedirectToAction("Index", "Home");
	}

	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		return RedirectToAction("Login");
	}

	private bool VerifyPassword(string password, string hash)
	{
		using SHA256 sha = SHA256.Create();
		var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
		var inputHash = Convert.ToBase64String(bytes);
		return inputHash == hash;
	}
}
