using Gestion_Usuarios.Data;
using Gestion_Usuarios.Helpers;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Controllers
{
	[Authorize]
	public class DocentesController : Controller
	{
		private readonly ManagementRepository _repo;

		public DocentesController(ContextDb context)
		{
			_repo = new ManagementRepository(context);
		}

		public async Task<IActionResult> Index()
		{
			var listaDocentes = await GetDocentesAsync();
			return View("~/Views/Dashboard/Docentes/Index.cshtml", listaDocentes);
		}

		[HttpGet]
		public IActionResult Create(string? returnUrl = null)
		{
			var model = new DocenteEditViewModel
			{
				ReturnUrl = NavigationHelper.ResolveReturnUrl(this, returnUrl, Url.Action(nameof(Index)) ?? "/Docentes"),
				EsActivo = true,
				EstadoCodigo = "ACTIVO"
			};

			return View("~/Views/Dashboard/Docentes/Create.cshtml", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(DocenteEditViewModel model)
		{
			NormalizeModel(model);

			if (!ModelState.IsValid)
			{
				return View("~/Views/Dashboard/Docentes/Create.cshtml", model);
			}

			var persons = await _repo.ExecuteStoredProcedureAsync(
				"management_person_insert",
				new Dictionary<string, object>
				{
					{ "@FirstName", model.Nombre },
					{ "@LastNamePaternal", model.ApellidoPaterno },
					{ "@LastNameMaternal", DbValue(model.ApellidoMaterno) },
					{ "@Email", DbValue(model.Email) },
					{ "@Phone", DbValue(model.Telefono) }
				},
				r => ManagementRepository.GetValue<int>(r, "management_person_ID"));

			var personId = persons.FirstOrDefault();
			if (personId <= 0)
			{
				ModelState.AddModelError(string.Empty, "No se pudo crear el registro de persona.");
				return View("~/Views/Dashboard/Docentes/Create.cshtml", model);
			}

			await _repo.ExecuteNonQueryAsync("management_teacher_insert", new Dictionary<string, object>
			{
				{ "@TeacherPersonID", personId },
				{ "@EmployeeNumber", DbValue(model.NumeroEmpleado) },
				{ "@Teacher_StatusCode", model.EstadoCodigo }
			});

			TempData["TeacherSuccess"] = "Docente registrado correctamente.";
			return RedirectToReturnUrl(model.ReturnUrl);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id, string? returnUrl = null)
		{
			var model = await GetDocenteByIdAsync(id);
			if (model == null)
			{
				TempData["TeacherError"] = "No se encontro el docente solicitado.";
				return RedirectToAction(nameof(Index));
			}

			model.ReturnUrl = NavigationHelper.ResolveReturnUrl(this, returnUrl, Url.Action(nameof(Index)) ?? "/Docentes");
			return View("~/Views/Dashboard/Docentes/Edit.cshtml", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(DocenteEditViewModel model)
		{
			NormalizeModel(model);

			if (!ModelState.IsValid)
			{
				return View("~/Views/Dashboard/Docentes/Edit.cshtml", model);
			}

			await _repo.ExecuteNonQueryAsync("management_person_update", new Dictionary<string, object>
			{
				{ "@ID", model.PersonId },
				{ "@FirstName", model.Nombre },
				{ "@LastNamePaternal", model.ApellidoPaterno },
				{ "@LastNameMaternal", DbValue(model.ApellidoMaterno) },
				{ "@Email", DbValue(model.Email) },
				{ "@Phone", DbValue(model.Telefono) }
			});

			await _repo.ExecuteNonQueryAsync("management_teacher_update", new Dictionary<string, object>
			{
				{ "@ID", model.Id },
				{ "@TeacherPersonID", model.PersonId },
				{ "@EmployeeNumber", DbValue(model.NumeroEmpleado) },
				{ "@Teacher_StatusCode", model.EstadoCodigo }
			});

			TempData["TeacherSuccess"] = "Docente actualizado correctamente.";
			return RedirectToReturnUrl(model.ReturnUrl);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _repo.ExecuteNonQueryAsync("management_teacher_softdelete", new Dictionary<string, object> { { "@ID", id } });
				return Ok(new { success = true, message = "Docente desactivado correctamente." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = ex.Message });
			}
		}

		private async Task<List<DocenteViewModel>> GetDocentesAsync()
		{
			const string docentesQuery = @"
SELECT
    COALESCE(u.management_user_ID, t.management_teacher_ID) AS management_user_ID,
    COALESCE(u.management_user_Email, p.management_person_Email) AS management_user_Email,
    p.management_person_Email AS person_email,
    p.management_person_FirstName,
    p.management_person_LastNamePaternal,
    p.management_person_LastNameMaternal,
    p.management_person_Phone,
    t.management_teacher_ID AS teacher_ID,
    t.management_teacher_EmployeeNumber,
    t.management_teacher_StatusCode AS teacher_statuscode,
    t.management_teacher_status AS teacher_status
FROM dbo.management_teacher_table t
INNER JOIN dbo.management_person_table p
    ON p.management_person_ID = t.management_teacher_PersonID
LEFT JOIN dbo.management_user_table u
    ON u.management_user_PersonID = p.management_person_ID
WHERE t.management_teacher_status IN (0, 1)
ORDER BY t.management_teacher_ID DESC;";

			return await _repo.ExecuteQueryAsync(docentesQuery, null, ModelMappers.MapToDocente);
		}

		private async Task<DocenteEditViewModel?> GetDocenteByIdAsync(int id)
		{
			const string sql = @"
SELECT
    t.management_teacher_ID,
    t.management_teacher_PersonID,
    t.management_teacher_EmployeeNumber,
    t.management_teacher_StatusCode,
    t.management_teacher_status,
    p.management_person_FirstName,
    p.management_person_LastNamePaternal,
    p.management_person_LastNameMaternal,
    p.management_person_Email,
    p.management_person_Phone
FROM dbo.management_teacher_table t
INNER JOIN dbo.management_person_table p
    ON p.management_person_ID = t.management_teacher_PersonID
WHERE t.management_teacher_ID = @ID;";

			var items = await _repo.ExecuteQueryAsync(
				sql,
				new Dictionary<string, object> { { "@ID", id } },
				reader => new DocenteEditViewModel
				{
					Id = ManagementRepository.GetValue<int>(reader, "management_teacher_ID"),
					PersonId = ManagementRepository.GetValue<int>(reader, "management_teacher_PersonID"),
					NumeroEmpleado = ManagementRepository.GetValue<string>(reader, "management_teacher_EmployeeNumber"),
					EstadoCodigo = ManagementRepository.GetValue<string>(reader, "management_teacher_StatusCode") ?? "ACTIVO",
					EsActivo = ManagementRepository.GetValue<bool>(reader, "management_teacher_status"),
					Nombre = ManagementRepository.GetValue<string>(reader, "management_person_FirstName") ?? string.Empty,
					ApellidoPaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNamePaternal") ?? string.Empty,
					ApellidoMaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNameMaternal"),
					Email = ManagementRepository.GetValue<string>(reader, "management_person_Email"),
					Telefono = ManagementRepository.GetValue<string>(reader, "management_person_Phone")
				});

			return items.FirstOrDefault();
		}

		private IActionResult RedirectToReturnUrl(string? returnUrl)
		{
			return NavigationHelper.RedirectToReturnUrl(this, returnUrl, nameof(Index));
		}

		private static void NormalizeModel(DocenteEditViewModel model)
		{
			model.Nombre = model.Nombre?.Trim() ?? string.Empty;
			model.ApellidoPaterno = model.ApellidoPaterno?.Trim() ?? string.Empty;
			model.ApellidoMaterno = NormalizeOptional(model.ApellidoMaterno);
			model.Email = NormalizeOptional(model.Email);
			model.Telefono = NormalizeOptional(model.Telefono);
			model.NumeroEmpleado = NormalizeOptional(model.NumeroEmpleado);
			model.EstadoCodigo = string.IsNullOrWhiteSpace(model.EstadoCodigo)
				? "ACTIVO"
				: model.EstadoCodigo.Trim().ToUpperInvariant();
		}

		private static string? NormalizeOptional(string? value)
		{
			return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
		}

		private static object DbValue(string? value)
		{
			return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
		}
	}
}
