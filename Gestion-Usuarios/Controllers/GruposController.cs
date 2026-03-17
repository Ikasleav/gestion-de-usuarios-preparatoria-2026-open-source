using Gestion_Usuarios.Data;
using Gestion_Usuarios.Helpers;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class GruposController : Controller
{
	private readonly ManagementRepository _repo;

	public GruposController(ContextDb context) => _repo = new ManagementRepository(context);

	public async Task<IActionResult> Index()
	{
		var lista = await _repo.ExecuteStoredProcedureAsync("management_group_get", null, ModelMappers.MapToGroup);
		return View("~/Views/Dashboard/Grupos/Index.cshtml", lista);
	}

	[HttpGet]
	public async Task<JsonResult> GetGrupos()
	{
		var lista = await _repo.ExecuteStoredProcedureAsync("management_group_get", null, ModelMappers.MapToGroup);
		return Json(new { data = lista });
	}

	[HttpGet]
	public async Task<IActionResult> Create(string? returnUrl = null)
	{
		var model = new GroupEditViewModel
		{
			ReturnUrl = NavigationHelper.ResolveReturnUrl(this, returnUrl, Url.Action(nameof(Index)) ?? "/Grupos"),
			EsActivo = true
		};

		await LoadCareerOptionsAsync(model);
		return View("~/Views/Dashboard/Grupos/Create.cshtml", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(GroupEditViewModel model)
	{
		NormalizeModel(model);

		if (!ModelState.IsValid)
		{
			await LoadCareerOptionsAsync(model);
			return View("~/Views/Dashboard/Grupos/Create.cshtml", model);
		}

		await _repo.ExecuteNonQueryAsync("management_group_insert", new Dictionary<string, object>
		{
			{ "@GroupCareerID", model.CareerId.HasValue ? model.CareerId.Value : DBNull.Value },
			{ "@GroupCode", model.Codigo },
			{ "@GroupName", DbValue(model.Nombre) },
			{ "@GroupShift", DbValue(model.Turno) }
		});

		TempData["GroupSuccess"] = "Grupo registrado correctamente.";
		return RedirectToReturnUrl(model.ReturnUrl);
	}

	[HttpGet]
	public async Task<IActionResult> Edit(int id, string? returnUrl = null)
	{
		var model = await GetGroupByIdAsync(id);
		if (model == null)
		{
			TempData["GroupError"] = "No se encontro el grupo solicitado.";
			return RedirectToAction(nameof(Index));
		}

		model.ReturnUrl = NavigationHelper.ResolveReturnUrl(this, returnUrl, Url.Action(nameof(Index)) ?? "/Grupos");
		await LoadCareerOptionsAsync(model);
		return View("~/Views/Dashboard/Grupos/Edit.cshtml", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(GroupEditViewModel model)
	{
		NormalizeModel(model);

		if (!ModelState.IsValid)
		{
			await LoadCareerOptionsAsync(model);
			return View("~/Views/Dashboard/Grupos/Edit.cshtml", model);
		}

		await _repo.ExecuteNonQueryAsync("management_group_update", new Dictionary<string, object>
		{
			{ "@ID", model.Id },
			{ "@GroupCareerID", model.CareerId.HasValue ? model.CareerId.Value : DBNull.Value },
			{ "@GroupCode", model.Codigo },
			{ "@GroupName", DbValue(model.Nombre) },
			{ "@GroupShift", DbValue(model.Turno) }
		});

		TempData["GroupSuccess"] = "Grupo actualizado correctamente.";
		return RedirectToReturnUrl(model.ReturnUrl);
	}

	[HttpPost]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			await _repo.ExecuteNonQueryAsync("management_group_softdelete", new Dictionary<string, object> { { "@ID", id } });
			return Ok(new { success = true, message = "Grupo dado de baja correctamente." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	private async Task<GroupEditViewModel?> GetGroupByIdAsync(int id)
	{
		const string sql = @"
SELECT
    management_group_ID,
    management_group_CareerID,
    management_group_Code,
    management_group_Name,
    management_group_Shift,
    management_group_status
FROM dbo.management_group_table
WHERE management_group_ID = @ID;";

		var items = await _repo.ExecuteQueryAsync(
			sql,
			new Dictionary<string, object> { { "@ID", id } },
			reader => new GroupEditViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_group_ID"),
				CareerId = ManagementRepository.GetValue<int?>(reader, "management_group_CareerID"),
				Codigo = ManagementRepository.GetValue<string>(reader, "management_group_Code") ?? string.Empty,
				Nombre = ManagementRepository.GetValue<string>(reader, "management_group_Name"),
				Turno = ManagementRepository.GetValue<string>(reader, "management_group_Shift"),
				EsActivo = ManagementRepository.GetValue<bool>(reader, "management_group_status")
			});

		return items.FirstOrDefault();
	}

	private async Task LoadCareerOptionsAsync(GroupEditViewModel model)
	{
		const string sql = @"
SELECT management_career_ID, management_career_Name
FROM dbo.management_career_table
WHERE management_career_status = 1
ORDER BY management_career_Name;";

		model.CareerOptions = await _repo.ExecuteQueryAsync(
			sql,
			null,
			reader => new LookupOptionViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_career_ID"),
				Name = ManagementRepository.GetValue<string>(reader, "management_career_Name") ?? string.Empty
			});
	}

	private IActionResult RedirectToReturnUrl(string? returnUrl)
	{
		return NavigationHelper.RedirectToReturnUrl(this, returnUrl, nameof(Index));
	}

	private static void NormalizeModel(GroupEditViewModel model)
	{
		model.Codigo = model.Codigo?.Trim() ?? string.Empty;
		model.Nombre = NormalizeOptional(model.Nombre);
		model.Turno = NormalizeOptional(model.Turno);
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
