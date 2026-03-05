using Gestion_Usuarios.Data;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class BajasController : Controller
{
	private readonly ManagementRepository _repo;
	public BajasController(ContextDb context) => _repo = new ManagementRepository(context);

	public async Task<IActionResult> Index()
	{
		// Filtramos por Status = 0 (Bajas) desde el repositorio
		var lista = await _repo.ExecuteStoredProcedureAsync(
			"getview_student_full",
			new Dictionary<string, object> { { "@Status", 0 } },
			ModelMappers.MapToStudent
		);

		return View("~/Views/Dashboard/Alumnos/Bajas.cshtml", lista);
	}
}