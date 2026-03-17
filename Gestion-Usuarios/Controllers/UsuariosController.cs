using System.Security.Cryptography;
using System.Text;
using Gestion_Usuarios.Data;
using Gestion_Usuarios.Helpers;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[Authorize]
public class UsuariosController : Controller
{
	private readonly ContextDb _context;
	private readonly ManagementRepository _repo;
	private readonly ILogger<UsuariosController> _logger;

	public UsuariosController(ContextDb context, ILogger<UsuariosController> logger)
	{
		_context = context;
		_repo = new ManagementRepository(context);
		_logger = logger;
	}

	public IActionResult Index() => View("~/Views/Dashboard/Usuarios/Index.cshtml");

	[HttpGet]
	public async Task<IActionResult> GetUsersJson()
	{
		try
		{
			var lista = await _repo.ExecuteStoredProcedureAsync(
				"getview_user_full",
				null,
				ModelMappers.MapToUsuario
			);

			_logger.LogInformation("GetUsersJson returned {Count} items", lista?.Count ?? 0);
			return Json(new { data = lista, count = lista?.Count ?? 0 });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in GetUsersJson");
			return StatusCode(500, new { error = ex.Message });
		}
	}

	[HttpGet]
	public async Task<IActionResult> Create(string? returnUrl = null)
	{
		var model = new UsuarioEditViewModel
		{
			ReturnUrl = NavigationHelper.ResolveReturnUrl(this, returnUrl, Url.Action(nameof(Index)) ?? "/Usuarios"),
			EsActivo = true
		};

		await LoadFormOptionsAsync(model);
		return View("~/Views/Dashboard/Usuarios/Create.cshtml", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(UsuarioEditViewModel model)
	{
		NormalizeModel(model);

		if (string.IsNullOrWhiteSpace(model.Password))
		{
			ModelState.AddModelError(nameof(model.Password), "La contrasena es obligatoria.");
		}

		if (_context.ManagementUsers.Any(u => u.management_user_Username == model.Username))
		{
			ModelState.AddModelError(nameof(model.Username), "El nombre de usuario ya existe.");
		}

		if (!string.IsNullOrWhiteSpace(model.Email) && _context.ManagementUsers.Any(u => u.management_user_Email == model.Email))
		{
			ModelState.AddModelError(nameof(model.Email), "El correo ya esta registrado.");
		}

		if (!ModelState.IsValid)
		{
			await LoadFormOptionsAsync(model);
			return View("~/Views/Dashboard/Usuarios/Create.cshtml", model);
		}

		var persons = await _repo.ExecuteStoredProcedureAsync(
			"management_person_insert",
			new Dictionary<string, object>
			{
				{ "@FirstName", model.Nombre },
				{ "@LastNamePaternal", model.ApellidoPaterno },
				{ "@LastNameMaternal", DbValue(model.ApellidoMaterno) },
				{ "@CURP", DbValue(model.CURP) },
				{ "@Email", DbValue(model.Email) },
				{ "@Phone", DbValue(model.Telefono) }
			},
			r => ManagementRepository.GetValue<int>(r, "management_person_ID"));

		var personId = persons.FirstOrDefault();
		if (personId <= 0)
		{
			ModelState.AddModelError(string.Empty, "No se pudo crear el registro de persona.");
			await LoadFormOptionsAsync(model);
			return View("~/Views/Dashboard/Usuarios/Create.cshtml", model);
		}

		var users = await _repo.ExecuteStoredProcedureAsync(
			"management_user_create_with_role",
			new Dictionary<string, object>
			{
				{ "@UserPersonID", personId },
				{ "@Username", model.Username },
				{ "@UserEmail", DbValue(model.Email) },
				{ "@PasswordHash", HashPassword(model.Password!) },
				{ "@IsLocked", model.IsLocked ? 1 : 0 },
				{ "@LockReason", DbValue(model.LockReason) },
				{ "@UserRole_RoleID", model.RoleId!.Value }
			},
			r => ManagementRepository.GetValue<int>(r, "management_user_ID"));

		var userId = users.FirstOrDefault();
		if (userId <= 0)
		{
			ModelState.AddModelError(string.Empty, "No se pudo crear el usuario.");
			await LoadFormOptionsAsync(model);
			return View("~/Views/Dashboard/Usuarios/Create.cshtml", model);
		}

		var roleName = await GetRoleNameAsync(model.RoleId.Value);
		await CreateUserCareerAsync(userId, model.CareerId!.Value, roleName);

		TempData["UserSuccess"] = "Usuario registrado correctamente.";
		return RedirectToReturnUrl(model.ReturnUrl);
	}

	[HttpGet]
	public async Task<IActionResult> Edit(int id, string? returnUrl = null)
	{
		var model = await GetUserByIdAsync(id);
		if (model == null)
		{
			TempData["UserError"] = "No se encontro el usuario solicitado.";
			return RedirectToAction(nameof(Index));
		}

		model.ReturnUrl = NavigationHelper.ResolveReturnUrl(this, returnUrl, Url.Action(nameof(Index)) ?? "/Usuarios");
		await LoadFormOptionsAsync(model);
		return View("~/Views/Dashboard/Usuarios/Edit.cshtml", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(UsuarioEditViewModel model)
	{
		NormalizeModel(model);

		if (_context.ManagementUsers.Any(u => u.management_user_ID != model.Id && u.management_user_Username == model.Username))
		{
			ModelState.AddModelError(nameof(model.Username), "El nombre de usuario ya existe.");
		}

		if (!string.IsNullOrWhiteSpace(model.Email) &&
			_context.ManagementUsers.Any(u => u.management_user_ID != model.Id && u.management_user_Email == model.Email))
		{
			ModelState.AddModelError(nameof(model.Email), "El correo ya esta registrado.");
		}

		if (!ModelState.IsValid)
		{
			await LoadFormOptionsAsync(model);
			return View("~/Views/Dashboard/Usuarios/Edit.cshtml", model);
		}

		var personId = model.PersonId ?? 0;
		if (personId <= 0)
		{
			var persons = await _repo.ExecuteStoredProcedureAsync(
				"management_person_insert",
				new Dictionary<string, object>
				{
					{ "@FirstName", model.Nombre },
					{ "@LastNamePaternal", model.ApellidoPaterno },
					{ "@LastNameMaternal", DbValue(model.ApellidoMaterno) },
					{ "@CURP", DbValue(model.CURP) },
					{ "@Email", DbValue(model.Email) },
					{ "@Phone", DbValue(model.Telefono) }
				},
				r => ManagementRepository.GetValue<int>(r, "management_person_ID"));

			personId = persons.FirstOrDefault();
		}
		else
		{
			await _repo.ExecuteNonQueryAsync("management_person_update", new Dictionary<string, object>
			{
				{ "@ID", personId },
				{ "@FirstName", model.Nombre },
				{ "@LastNamePaternal", model.ApellidoPaterno },
				{ "@LastNameMaternal", DbValue(model.ApellidoMaterno) },
				{ "@CURP", DbValue(model.CURP) },
				{ "@Email", DbValue(model.Email) },
				{ "@Phone", DbValue(model.Telefono) }
			});
		}

		var userParams = new Dictionary<string, object>
		{
			{ "@ID", model.Id },
			{ "@UserPersonID", personId > 0 ? personId : DBNull.Value },
			{ "@Username", model.Username },
			{ "@UserEmail", DbValue(model.Email) },
			{ "@IsLocked", model.IsLocked ? 1 : 0 },
			{ "@LockReason", DbValue(model.LockReason) }
		};

		if (!string.IsNullOrWhiteSpace(model.Password))
		{
			userParams["@PasswordHash"] = HashPassword(model.Password);
		}

		await _repo.ExecuteNonQueryAsync("management_user_update", userParams);

		await _repo.ExecuteCommandAsync(
			"UPDATE dbo.management_userrole_table SET management_userrole_status = 0 WHERE management_userrole_UserID = @UserID AND management_userrole_status = 1;",
			new Dictionary<string, object> { { "@UserID", model.Id } });

		await _repo.ExecuteNonQueryAsync("management_userrole_insert", new Dictionary<string, object>
		{
			{ "@UserRole_UserID", model.Id },
			{ "@UserRole_RoleID", model.RoleId!.Value }
		});

		await _repo.ExecuteCommandAsync(
			"UPDATE dbo.management_usercareer_table SET management_usercareer_status = 0 WHERE management_usercareer_UserID = @UserID AND management_usercareer_status = 1;",
			new Dictionary<string, object> { { "@UserID", model.Id } });

		var roleName = await GetRoleNameAsync(model.RoleId.Value);
		await CreateUserCareerAsync(model.Id, model.CareerId!.Value, roleName);

		TempData["UserSuccess"] = "Usuario actualizado correctamente.";
		return RedirectToReturnUrl(model.ReturnUrl);
	}

	[HttpPost]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			await _repo.ExecuteNonQueryAsync("management_user_softdelete", new Dictionary<string, object> { { "@ID", id } });
			return Ok(new { success = true, message = "Usuario dado de baja correctamente." });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting user {Id}", id);
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	private async Task<UsuarioEditViewModel?> GetUserByIdAsync(int id)
	{
		const string sql = @"
SELECT
    u.management_user_ID,
    u.management_user_PersonID,
    u.management_user_Username,
    u.management_user_Email,
    u.management_user_IsLocked,
    u.management_user_LockReason,
    u.management_user_status,
    p.management_person_FirstName,
    p.management_person_LastNamePaternal,
    p.management_person_LastNameMaternal,
    p.management_person_CURP,
    p.management_person_Phone,
    ur.management_userrole_RoleID,
    uc.management_usercareer_CareerID
FROM dbo.management_user_table u
LEFT JOIN dbo.management_person_table p
    ON p.management_person_ID = u.management_user_PersonID
OUTER APPLY (
    SELECT TOP 1 management_userrole_RoleID
    FROM dbo.management_userrole_table
    WHERE management_userrole_UserID = u.management_user_ID
      AND management_userrole_status = 1
    ORDER BY management_userrole_createdDate DESC, management_userrole_ID DESC
) ur
OUTER APPLY (
    SELECT TOP 1 management_usercareer_CareerID
    FROM dbo.management_usercareer_table
    WHERE management_usercareer_UserID = u.management_user_ID
      AND management_usercareer_status = 1
    ORDER BY management_usercareer_createdDate DESC, management_usercareer_ID DESC
) uc
WHERE u.management_user_ID = @ID;";

		var items = await _repo.ExecuteQueryAsync(
			sql,
			new Dictionary<string, object> { { "@ID", id } },
			reader => new UsuarioEditViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_user_ID"),
				PersonId = ManagementRepository.GetValue<int?>(reader, "management_user_PersonID"),
				Username = ManagementRepository.GetValue<string>(reader, "management_user_Username") ?? string.Empty,
				Email = ManagementRepository.GetValue<string>(reader, "management_user_Email"),
				IsLocked = ManagementRepository.GetValue<bool>(reader, "management_user_IsLocked"),
				LockReason = ManagementRepository.GetValue<string>(reader, "management_user_LockReason"),
				EsActivo = ManagementRepository.GetValue<bool>(reader, "management_user_status"),
				Nombre = ManagementRepository.GetValue<string>(reader, "management_person_FirstName") ?? string.Empty,
				ApellidoPaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNamePaternal") ?? string.Empty,
				ApellidoMaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNameMaternal"),
				CURP = ManagementRepository.GetValue<string>(reader, "management_person_CURP"),
				Telefono = ManagementRepository.GetValue<string>(reader, "management_person_Phone"),
				RoleId = ManagementRepository.GetValue<int?>(reader, "management_userrole_RoleID"),
				CareerId = ManagementRepository.GetValue<int?>(reader, "management_usercareer_CareerID")
			});

		return items.FirstOrDefault();
	}

	private async Task LoadFormOptionsAsync(UsuarioEditViewModel model)
	{
		const string rolesSql = @"
SELECT management_role_ID, management_role_Name
FROM dbo.management_role_table
WHERE management_role_status = 1
ORDER BY management_role_Name;";

		model.RoleOptions = await _repo.ExecuteQueryAsync(
			rolesSql,
			null,
			reader => new LookupOptionViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_role_ID"),
				Name = ManagementRepository.GetValue<string>(reader, "management_role_Name") ?? string.Empty
			});

		const string careersSql = @"
SELECT management_career_ID, management_career_Name
FROM dbo.management_career_table
WHERE management_career_status = 1
ORDER BY management_career_Name;";

		model.CareerOptions = await _repo.ExecuteQueryAsync(
			careersSql,
			null,
			reader => new LookupOptionViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_career_ID"),
				Name = ManagementRepository.GetValue<string>(reader, "management_career_Name") ?? string.Empty
			});
	}

	private async Task<string> GetRoleNameAsync(int roleId)
	{
		const string sql = @"
SELECT management_role_Name
FROM dbo.management_role_table
WHERE management_role_ID = @ID;";

		var roles = await _repo.ExecuteQueryAsync(
			sql,
			new Dictionary<string, object> { { "@ID", roleId } },
			reader => ManagementRepository.GetValue<string>(reader, "management_role_Name") ?? string.Empty);

		return roles.FirstOrDefault() ?? string.Empty;
	}

	private async Task CreateUserCareerAsync(int userId, int careerId, string roleName)
	{
		await _repo.ExecuteNonQueryAsync("management_usercareer_insert", new Dictionary<string, object>
		{
			{ "@UserCareer_UserID", userId },
			{ "@UserCareer_CareerID", careerId },
			{ "@RoleInCareer", string.IsNullOrWhiteSpace(roleName) ? "USUARIO" : roleName }
		});
	}

	private IActionResult RedirectToReturnUrl(string? returnUrl)
	{
		return NavigationHelper.RedirectToReturnUrl(this, returnUrl, nameof(Index));
	}

	private static void NormalizeModel(UsuarioEditViewModel model)
	{
		model.Username = model.Username?.Trim() ?? string.Empty;
		model.Email = NormalizeOptional(model.Email);
		model.Nombre = model.Nombre?.Trim() ?? string.Empty;
		model.ApellidoPaterno = model.ApellidoPaterno?.Trim() ?? string.Empty;
		model.ApellidoMaterno = NormalizeOptional(model.ApellidoMaterno);
		model.CURP = NormalizeOptional(model.CURP)?.ToUpperInvariant();
		model.Telefono = NormalizeOptional(model.Telefono);
		model.LockReason = NormalizeOptional(model.LockReason);
	}

	private static string? NormalizeOptional(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
	}

	private static object DbValue(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
	}

	private static string HashPassword(string password)
	{
		using SHA256 sha = SHA256.Create();
		return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
	}
}
