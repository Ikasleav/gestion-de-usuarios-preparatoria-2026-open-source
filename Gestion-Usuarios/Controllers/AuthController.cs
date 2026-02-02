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

		// Login exitoso
		user.management_user_LastLoginDate = DateTime.Now;
		_context.SaveChanges();

		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, user.management_user_ID.ToString()),
			new Claim(ClaimTypes.Name, user.management_user_Username)
		};

		if (!string.IsNullOrEmpty(user.management_user_Email))
			claims.Add(new Claim(ClaimTypes.Email, user.management_user_Email));

		// Add a default role claim if needed (roles not stored in DB yet)
		claims.Add(new Claim(ClaimTypes.Role, "Student"));

		var identity = new ClaimsIdentity(
			claims,
			CookieAuthenticationDefaults.AuthenticationScheme);

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(identity));

		return RedirectToAction("Index", "Home");
	}

	[HttpGet]
	public IActionResult Register()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Register(RegisterViewModel model)
	{
		if (!ModelState.IsValid)
			return View(model);

		// Check username/email uniqueness
		if (_context.ManagementUsers.Any(u => u.management_user_Username == model.Username))
		{
			ModelState.AddModelError(nameof(model.Username), "El nombre de usuario ya existe.");
			return View(model);
		}

		if (_context.ManagementUsers.Any(u => u.management_user_Email == model.Email))
		{
			ModelState.AddModelError(nameof(model.Email), "El correo ya está registrado.");
			return View(model);
		}

		// Create user with hashed password
		var user = new ManagementUser
		{
			management_user_Username = model.Username,
			management_user_Email = model.Email,
			management_user_PasswordHash = HashPassword(model.Password),
			management_user_IsLocked = false,
			management_user_status = true,
			management_user_createdDate = DateTime.Now
			// management_user_PersonID can be set later when you create the person record
		};

		_context.ManagementUsers.Add(user);
		_context.SaveChanges();

		// Optionally auto-login after registration
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, user.management_user_ID.ToString()),
			new Claim(ClaimTypes.Name, user.management_user_Username),
			new Claim(ClaimTypes.Email, user.management_user_Email ?? string.Empty),
			new Claim(ClaimTypes.Role, "Student")
		};

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

	private string HashPassword(string password)
	{
		using SHA256 sha = SHA256.Create();
		var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
		return Convert.ToBase64String(bytes);
	}
}
