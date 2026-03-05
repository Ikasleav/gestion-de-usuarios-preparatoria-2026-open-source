using Gestion_Usuarios.Data;
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

	[HttpPost]
	public async Task<IActionResult> Delete(int id)
	{
		await _repo.ExecuteNonQueryAsync("management_group_softdelete", new Dictionary<string, object> { { "@ID", id } });
		return Ok(new { success = true });
	}
}