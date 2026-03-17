using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Gestion_Usuarios.Data;
using Gestion_Usuarios.Models;
using Gestion_Usuarios.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Gestion_Usuarios.Controllers
{
    public class AuthController : Controller
    {
        private readonly ContextDb _context;
        private readonly ManagementRepository _repo;

        public AuthController(ContextDb context)
        {
            _context = context;
            _repo = new ManagementRepository(context);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await BuildAuthViewModelAsync());
        }

        [HttpGet]
        public IActionResult Login()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ActiveForm = "login";
                return View("Index", await BuildAuthViewModelAsync(login: model));
            }

            var user = _context.ManagementUsers.FirstOrDefault(u =>
                u.management_user_Username == model.UserOrEmail ||
                u.management_user_Email == model.UserOrEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o correo incorrecto");
                ViewBag.ActiveForm = "login";
                return View("Index", await BuildAuthViewModelAsync(login: model));
            }

            if (user.management_user_IsLocked)
            {
                ModelState.AddModelError(string.Empty, "Usuario bloqueado");
                ViewBag.ActiveForm = "login";
                return View("Index", await BuildAuthViewModelAsync(login: model));
            }

            if (!user.management_user_status)
            {
                ModelState.AddModelError(string.Empty, "Usuario inactivo");
                ViewBag.ActiveForm = "login";
                return View("Index", await BuildAuthViewModelAsync(login: model));
            }

            if (!VerifyPassword(model.Password, user.management_user_PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Contrasena incorrecta");
                ViewBag.ActiveForm = "login";
                return View("Index", await BuildAuthViewModelAsync(login: model));
            }

            user.management_user_LastLoginDate = DateTime.Now;
            _context.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.management_user_ID.ToString()),
                new Claim(ClaimTypes.Name, user.management_user_Username)
            };

            if (!string.IsNullOrEmpty(user.management_user_Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.management_user_Email));
            }

            AddRoleClaims(claims, await GetRoleNamesAsync(user.management_user_ID));

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ActiveForm = "register";
                return View("Index", await BuildAuthViewModelAsync(register: model));
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    NormalizeRegisterModel(model);

                    // 1. Insertar Persona
                    var pNombre = new SqlParameter("@FirstName", model.Nombre);
                    var pApePat = new SqlParameter("@LastNamePaternal", model.ApellidoPaterno);
                    var pApeMat = new SqlParameter("@LastNameMaternal", (object)model.ApellidoMaterno ?? DBNull.Value);
                    var pEmail = new SqlParameter("@Email", model.Email);

                    // Usamos ExecuteSqlRaw para obtener el ID recién creado
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO dbo.management_person_table (management_person_FirstName, management_person_LastNamePaternal, management_person_LastNameMaternal, management_person_Email) " +
                        "VALUES (@FirstName, @LastNamePaternal, @LastNameMaternal, @Email)",
                        pNombre, pApePat, pApeMat, pEmail);

                    // Obtenemos el ID de la persona (Asegúrate de tener el modelo ManagementPerson creado)
                    var newPersonId = _context.ManagementPersons.Max(p => p.management_person_ID);

                    // 2. Insertar Usuario
                    string passwordHash = HashPassword(model.Password);
                    var newUser = new ManagementUser
                    {
                        management_user_PersonID = newPersonId,
                        management_user_Username = model.Username,
                        management_user_Email = model.Email,
                        management_user_PasswordHash = passwordHash,
                        management_user_status = true,
                        management_user_createdDate = DateTime.Now
                    };
                    _context.ManagementUsers.Add(newUser);
                    await _context.SaveChangesAsync();

                    int newUserId = newUser.management_user_ID;

                    // 3. Insertar Rol (CORREGIDO: management_userrole_table)
                    // Según tu SQL, el rol 3 es 'STUDENT'. Cámbialo si necesitas otro.
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO dbo.management_userrole_table (management_userrole_UserID, management_userrole_RoleID, management_userrole_status) " +
                        "VALUES ({0}, {1}, {2})", newUserId, 3, 1);

                    await transaction.CommitAsync();

                    TempData["Success"] = "¡Cuenta creada! Ya puedes entrar.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Esto te ayudará a ver en consola si hay otro error de nombre
                    Debug.WriteLine("Error en registro: " + ex.Message);
                    ModelState.AddModelError("", "Error al registrar: " + ex.Message);
                    ViewBag.ActiveForm = "register";
                    return View("Index", await BuildAuthViewModelAsync(register: model));
                }
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        private async Task<CombinedAuthViewModel> BuildAuthViewModelAsync(
            LoginViewModel? login = null,
            RegisterViewModel? register = null)
        {
            register ??= new RegisterViewModel();

            return new CombinedAuthViewModel
            {
                Login = login,
                Register = register
            };
        }

        private async Task<(int RoleId, string RoleName)> ResolveRegisterRoleAsync()
        {
            const string rolesSql = @"
            SELECT management_role_ID, management_role_Name
            FROM dbo.management_role_table
            WHERE management_role_status = 1
            ORDER BY management_role_Name;";

            var roles = await _repo.ExecuteQueryAsync(
                rolesSql,
                null,
                reader => new LookupOptionViewModel
                {
                    Id = ManagementRepository.GetValue<int>(reader, "management_role_ID"),
                    Name = ManagementRepository.GetValue<string>(reader, "management_role_Name") ?? string.Empty
                });

            var activeAdminExists = await _repo.ExecuteQueryAsync(
                @"
                SELECT TOP 1 1 AS ExistsFlag
                FROM dbo.management_userrole_table ur
                INNER JOIN dbo.management_role_table r
                    ON r.management_role_ID = ur.management_userrole_RoleID
                WHERE ur.management_userrole_status = 1
                  AND r.management_role_status = 1
                  AND UPPER(r.management_role_Name) IN ('ADMIN', 'ADMINISTRADOR', 'ADMINISTRATOR');",
                null,
                reader => ManagementRepository.GetValue<int>(reader, "ExistsFlag"));

            var bootstrapRole = !activeAdminExists.Any()
                ? roles.FirstOrDefault(r =>
                    string.Equals(r.Name, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r.Name, "ADMINISTRADOR", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r.Name, "ADMINISTRATOR", StringComparison.OrdinalIgnoreCase))
                : null;

            var defaultRole = bootstrapRole ?? roles.FirstOrDefault(r =>
                string.Equals(r.Name, "STUDENT", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r.Name, "ALUMNO", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r.Name, "USUARIO", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r.Name, "USER", StringComparison.OrdinalIgnoreCase));

            return defaultRole is null
                ? (0, string.Empty)
                : (defaultRole.Id, defaultRole.Name);
        }

        private async Task<List<string>> GetRoleNamesAsync(int userId)
        {
            const string sql = @"
            SELECT DISTINCT r.management_role_Name
            FROM dbo.management_userrole_table ur
            INNER JOIN dbo.management_role_table r
                ON r.management_role_ID = ur.management_userrole_RoleID
            WHERE ur.management_userrole_UserID = @UserID
              AND ur.management_userrole_status = 1
              AND r.management_role_status = 1;";

            var roles = new List<string>();
            var conn = _context.Database.GetDbConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(new SqlParameter("@UserID", userId));

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            try
            {
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0))
                    {
                        var role = reader.GetString(0);
                        if (!string.IsNullOrWhiteSpace(role))
                        {
                            roles.Add(role.Trim());
                        }
                    }
                }
            }
            finally
            {
                await conn.CloseAsync();
            }

            return roles;
        }

        private static void AddRoleClaims(List<Claim> claims, IEnumerable<string> roles)
        {
            var distinctRoles = roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (distinctRoles.Count == 0)
            {
                distinctRoles.Add("Student");
            }

            foreach (var role in distinctRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

                var upperRole = role.ToUpperInvariant();
                if (!string.Equals(role, upperRole, StringComparison.Ordinal))
                {
                    claims.Add(new Claim(ClaimTypes.Role, upperRole));
                }
            }
        }

        private static void NormalizeRegisterModel(RegisterViewModel model)
        {
            model.Username = model.Username?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim() ?? string.Empty;
            model.Nombre = model.Nombre?.Trim() ?? string.Empty;
            model.ApellidoPaterno = model.ApellidoPaterno?.Trim() ?? string.Empty;
            model.ApellidoMaterno = NormalizeOptional(model.ApellidoMaterno);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static object DbValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
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
}
