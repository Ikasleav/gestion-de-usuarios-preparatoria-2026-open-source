using Gestion_Usuarios.Data;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[Authorize]
public class HistoricosController : Controller
{
	private readonly ManagementRepository _repo;
	private readonly ILogger<HistoricosController> _logger;

	public HistoricosController(ContextDb context, ILogger<HistoricosController> logger)
	{
		_repo = new ManagementRepository(context);
		_logger = logger;
	}

	public async Task<IActionResult> Index()
	{
		var lista = await _repo.ExecuteStoredProcedureAsync("getview_user_full", null, ModelMappers.MapToHistorico);
		return View("~/Views/Dashboard/Historicos/Index.cshtml", lista);
	}

	[HttpGet]
	public async Task<JsonResult> GetHistorial()
	{
		try
		{
			var lista = await _repo.ExecuteStoredProcedureAsync("getview_user_full", null, ModelMappers.MapToHistorico);
			_logger.LogInformation("GetHistorial returned {Count} items", lista?.Count ?? 0);
			return Json(new { data = lista, count = lista?.Count ?? 0 });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in GetHistorial");
			Response.StatusCode = 500;
			return Json(new { error = ex.Message });
		}
	}
}