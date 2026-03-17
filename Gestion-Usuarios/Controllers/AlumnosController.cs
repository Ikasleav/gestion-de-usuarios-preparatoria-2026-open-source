using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gestion_Usuarios.Data;
using Gestion_Usuarios.Helpers;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gestion_Usuarios.Controllers
{
	[Authorize]
	public class AlumnosController : Controller
	{
		private const string StudentListOption = "getview_student_full";
		private readonly ManagementRepository _repo;
		private readonly ILogger<AlumnosController> _logger;

		public AlumnosController(ContextDb context, ILogger<AlumnosController> logger)
		{
			_repo = new ManagementRepository(context);
			_logger = logger;
		}

		public async Task<IActionResult> Index()
		{
			var lista = await GetStudentCatalogAsync();
			var alumnosActivos = lista
				.Where(IsActiveStudent)
				.OrderBy(a => a.ApellidoPaterno)
				.ThenBy(a => a.ApellidoMaterno)
				.ThenBy(a => a.Nombres)
				.ToList();

			_logger?.LogInformation("Alumnos Index: total {Total}, activos {Activos}", lista?.Count ?? 0, alumnosActivos.Count);
			return View("~/Views/Dashboard/Alumnos/Index.cshtml", alumnosActivos);
		}

		[HttpGet]
		public async Task<IActionResult> Create(string? returnUrl = null)
		{
			var model = new StudentEditViewModel
			{
				ReturnUrl = NavigationHelper.ResolveReturnUrl(this, returnUrl, Url.Action(nameof(Index)) ?? "/Alumnos"),
				EsActivo = true
			};

			await LoadLookupOptionsAsync(model);
			return View("~/Views/Dashboard/Alumnos/Create.cshtml", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(StudentEditViewModel model)
		{
			NormalizeEditModel(model);

			if (!ModelState.IsValid)
			{
				await LoadLookupOptionsAsync(model);
				return View("~/Views/Dashboard/Alumnos/Create.cshtml", model);
			}

			var (created, error) = await TryCreateStudentAsync(model);
			if (!created)
			{
				_logger.LogWarning("Create alumno fallo: {Error}", error ?? "sin mensaje");
				ModelState.AddModelError(string.Empty, "No se pudo crear el alumno. Revisa la opcion de insercion del procedimiento almacenado.");
				if (!string.IsNullOrWhiteSpace(error))
				{
					ModelState.AddModelError(string.Empty, error);
				}

				await LoadLookupOptionsAsync(model);
				return View("~/Views/Dashboard/Alumnos/Create.cshtml", model);
			}

			TempData["StudentSuccess"] = "Alumno registrado correctamente.";
			return RedirectToReturnUrl(model);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id, string? returnUrl = null)
		{
			if (id <= 0)
			{
				return RedirectToAction(nameof(Index));
			}

			var model = await GetStudentByIdAsync(id);
			if (model == null)
			{
				TempData["StudentError"] = "No se encontro el alumno solicitado.";
				if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
				{
					return LocalRedirect(returnUrl);
				}

				return RedirectToAction(nameof(Index));
			}

			model.ReturnUrl = NavigationHelper.ResolveReturnUrl(this, returnUrl, Url.Action(nameof(Index)) ?? "/Alumnos");
			await LoadLookupOptionsAsync(model);
			return View("~/Views/Dashboard/Alumnos/Edit.cshtml", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(StudentEditViewModel model)
		{
			NormalizeEditModel(model);

			if (!ModelState.IsValid)
			{
				await LoadLookupOptionsAsync(model);
				return View("~/Views/Dashboard/Alumnos/Edit.cshtml", model);
			}

			var updated = await TryUpdateStudentAsync(model);
			if (!updated)
			{
				ModelState.AddModelError(string.Empty, "No se pudo actualizar el alumno. Revisa la opcion de actualizacion del procedimiento almacenado.");
				await LoadLookupOptionsAsync(model);
				return View("~/Views/Dashboard/Alumnos/Edit.cshtml", model);
			}

			TempData["StudentSuccess"] = "Alumno actualizado correctamente.";
			return RedirectToReturnUrl(model);
		}

		[HttpPost]
		[Route("Alumnos/Delete/{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			if (id <= 0)
			{
				return BadRequest(new { success = false, message = "Identificador invalido." });
			}

			try
			{
				await _repo.ExecuteNonQueryAsync("management_student_softdelete", new Dictionary<string, object>
				{
					{ "@ID", id }
				});

				return Ok(new { success = true, message = "Alumno dado de baja correctamente." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al ejecutar soft delete para alumno {Id}", id);
				return StatusCode(500, new { success = false, message = "No se pudo procesar la baja del alumno." });
			}
		}

		private async Task<List<StudentViewModel>> GetStudentCatalogAsync()
		{
			return await _repo.ExecuteStoredProcedureAsync(
				StudentListOption,
				null,
				ModelMappers.MapToStudent);
		}

		private async Task<StudentEditViewModel?> GetStudentByIdAsync(int id)
		{
			const string sql = @"
SELECT
    s.management_student_ID,
    s.management_student_CareerID,
    s.management_student_GroupID,
    s.management_student_Matricula,
    s.management_student_EnrollmentFolio,
    s.management_student_StatusCode,
    s.management_student_status,
    p.management_person_FirstName,
    p.management_person_LastNamePaternal,
    p.management_person_LastNameMaternal,
    p.management_person_CURP,
    p.management_person_Email,
    TRY_CAST(LEFT(g.management_group_Code, 1) AS INT) AS Grado
FROM dbo.management_student_table s
INNER JOIN dbo.management_person_table p
    ON p.management_person_ID = s.management_student_PersonID
LEFT JOIN dbo.management_group_table g
    ON g.management_group_ID = s.management_student_GroupID
WHERE s.management_student_ID = @ID;";

			var items = await _repo.ExecuteQueryAsync(
				sql,
				new Dictionary<string, object> { { "@ID", id } },
				reader => new StudentEditViewModel
				{
					Id = ManagementRepository.GetValue<int>(reader, "management_student_ID"),
					CareerId = ManagementRepository.GetValue<int?>(reader, "management_student_CareerID"),
					GroupId = ManagementRepository.GetValue<int?>(reader, "management_student_GroupID"),
					Matricula = ManagementRepository.GetValue<string>(reader, "management_student_Matricula"),
					Folio = ManagementRepository.GetValue<string>(reader, "management_student_EnrollmentFolio"),
					Nombres = ManagementRepository.GetValue<string>(reader, "management_person_FirstName") ?? string.Empty,
					ApellidoPaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNamePaternal") ?? string.Empty,
					ApellidoMaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNameMaternal"),
					CURP = ManagementRepository.GetValue<string>(reader, "management_person_CURP"),
					Email = ManagementRepository.GetValue<string>(reader, "management_person_Email"),
					Semestre = ManagementRepository.GetValue<int?>(reader, "Grado"),
					EstadoCodigo = ManagementRepository.GetValue<string>(reader, "management_student_StatusCode") ?? string.Empty,
					EsActivo = ManagementRepository.GetValue<bool>(reader, "management_student_status")
				});

			return items.FirstOrDefault();
		}

		private async Task LoadLookupOptionsAsync(StudentEditViewModel model)
		{
			const string careersSql = @"
SELECT management_career_ID, management_career_Name
FROM dbo.management_career_table
WHERE management_career_status = 1
ORDER BY management_career_Name;";

			const string groupsSql = @"
SELECT
    management_group_ID,
    management_group_Code,
    management_group_Name,
    management_group_Shift
FROM dbo.management_group_table
WHERE management_group_status = 1
ORDER BY management_group_Code, management_group_Name;";

			model.CareerOptions = await _repo.ExecuteQueryAsync(
				careersSql,
				null,
				reader => new LookupOptionViewModel
				{
					Id = ManagementRepository.GetValue<int>(reader, "management_career_ID"),
					Name = ManagementRepository.GetValue<string>(reader, "management_career_Name") ?? string.Empty
				});

			model.GroupOptions = await _repo.ExecuteQueryAsync(
				groupsSql,
				null,
				reader =>
				{
					var code = ManagementRepository.GetValue<string>(reader, "management_group_Code") ?? string.Empty;
					var name = ManagementRepository.GetValue<string>(reader, "management_group_Name");
					var shift = ManagementRepository.GetValue<string>(reader, "management_group_Shift");
					var display = code;

					if (!string.IsNullOrWhiteSpace(name))
					{
						display += $" - {name}";
					}

					if (!string.IsNullOrWhiteSpace(shift))
					{
						display += $" ({shift})";
					}

					return new LookupOptionViewModel
					{
						Id = ManagementRepository.GetValue<int>(reader, "management_group_ID"),
						Name = display
					};
				});
		}

		private async Task<bool> TryUpdateStudentAsync(StudentEditViewModel model)
		{
			try
			{
				var getParams = new Dictionary<string, object> { { "@ID", model.Id } };
				var students = await _repo.ExecuteStoredProcedureAsync(
					"management_student_get",
					getParams,
					r => ManagementRepository.GetValue<int>(r, "management_student_PersonID"));

				var personId = students.FirstOrDefault();
				if (personId == 0)
				{
					return false;
				}

				var personParams = new Dictionary<string, object>
				{
					{ "@ID", personId },
					{ "@FirstName", model.Nombres },
					{ "@LastNamePaternal", model.ApellidoPaterno },
					{ "@LastNameMaternal", DbValue(model.ApellidoMaterno) },
					{ "@CURP", DbValue(model.CURP) },
					{ "@Email", DbValue(model.Email) }
				};
				await _repo.ExecuteNonQueryAsync("management_person_update", personParams);

				var studentParams = new Dictionary<string, object>
				{
					{ "@ID", model.Id },
					{ "@Student_Matricula", DbValue(model.Matricula) },
					{ "@Student_EnrollmentFolio", DbValue(model.Folio) },
					{ "@StudentCareerID", model.CareerId.HasValue ? model.CareerId.Value : DBNull.Value },
					{ "@StudentGroupID", model.GroupId.HasValue ? model.GroupId.Value : DBNull.Value }
				};

				await _repo.ExecuteNonQueryAsync("management_student_update", studentParams);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al actualizar el alumno {Id}", model.Id);
				return false;
			}
		}

		private async Task<(bool Success, string? Error)> TryCreateStudentAsync(StudentEditViewModel model)
		{
			try
			{
				var personParams = new Dictionary<string, object>
				{
					{ "@FirstName", model.Nombres },
					{ "@LastNamePaternal", model.ApellidoPaterno },
					{ "@LastNameMaternal", DbValue(model.ApellidoMaterno) },
					{ "@CURP", DbValue(model.CURP) },
					{ "@Email", DbValue(model.Email) }
				};

				var persons = await _repo.ExecuteStoredProcedureAsync(
					"management_person_insert",
					personParams,
					r => ManagementRepository.GetValue<int>(r, "management_person_ID"));

				var personId = persons.FirstOrDefault();
				if (personId == 0)
				{
					return (false, "Error al crear registro de persona.");
				}

				var isFolio = string.IsNullOrWhiteSpace(model.Matricula);

				var studentParams = new Dictionary<string, object>
				{
					{ "@StudentPersonID", personId },
					{ "@StudentCareerID", model.CareerId.HasValue ? model.CareerId.Value : DBNull.Value },
					{ "@StudentGroupID", model.GroupId.HasValue ? model.GroupId.Value : DBNull.Value },
					{ "@Student_IsFolio", isFolio ? 1 : 0 },
					{ "@Student_Matricula", DbValue(model.Matricula) },
					{ "@Student_EnrollmentFolio", DbValue(model.Folio) },
					{ "@ForceAutoNumbers", 1 }
				};

				await _repo.ExecuteNonQueryAsync("management_student_insert", studentParams);
				return (true, null);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en insercion automatica.");
				return (false, ex.Message);
			}
		}

		private IActionResult RedirectToReturnUrl(StudentEditViewModel model)
		{
			if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
			{
				return NavigationHelper.RedirectToReturnUrl(this, model.ReturnUrl, nameof(Index));
			}

			if (!model.EsActivo || string.Equals(model.EstadoCodigo, "BAJA", StringComparison.OrdinalIgnoreCase))
			{
				return RedirectToAction("Index", "Bajas");
			}

			if (string.IsNullOrWhiteSpace(model.Matricula) && !string.IsNullOrWhiteSpace(model.Folio))
			{
				return RedirectToAction("Index", "Nuevo_ingreso");
			}

			return RedirectToAction(nameof(Index));
		}

		private static void NormalizeEditModel(StudentEditViewModel model)
		{
			model.Matricula = NormalizeOptional(model.Matricula);
			model.Folio = NormalizeOptional(model.Folio);
			model.Nombres = model.Nombres?.Trim() ?? string.Empty;
			model.ApellidoPaterno = model.ApellidoPaterno?.Trim() ?? string.Empty;
			model.ApellidoMaterno = NormalizeOptional(model.ApellidoMaterno);
			model.CURP = NormalizeOptional(model.CURP)?.ToUpperInvariant();
			model.Email = NormalizeOptional(model.Email);
		}

		private static bool IsActiveStudent(StudentViewModel student)
		{
			var isBaja = string.Equals(student.EstadoCodigo, "BAJA", StringComparison.OrdinalIgnoreCase);
			var hasMatricula = !string.IsNullOrWhiteSpace(student.Matricula);

			return student.EsActivo && !isBaja && hasMatricula;
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
