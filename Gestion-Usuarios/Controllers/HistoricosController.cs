using Gestion_Usuarios.Data;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class HistoricosController : Controller
{
	private readonly ManagementRepository _repo;
	public HistoricosController(ContextDb context) => _repo = new ManagementRepository(context);

	public async Task<IActionResult> Index()
	{
		var lista = await _repo.ExecuteStoredProcedureAsync("getview_user_full", null, ModelMappers.MapToHistorico);
		return View("~/Views/Dashboard/Historicos/Index.cshtml", lista);
	}

	[HttpGet]
	public async Task<JsonResult> GetHistorial()
	{
		var lista = await _repo.ExecuteStoredProcedureAsync("getview_user_full", null, ModelMappers.MapToHistorico);
		return Json(new { data = lista });
	}
}