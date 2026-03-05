using Gestion_Usuarios.Data;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class Nuevo_ingresoController : Controller
{
	private readonly ManagementRepository _repo;
	public Nuevo_ingresoController(ContextDb context) => _repo = new ManagementRepository(context);

	public async Task<IActionResult> Index()
	{
		// Filtramos solo los que son Folio (Nuevo Ingreso)
		var lista = await _repo.ExecuteStoredProcedureAsync(
			"getview_student_full",
			new Dictionary<string, object> { { "@Student_IsFolio", 1 } },
			ModelMappers.MapToStudent
		);

		return View("~/Views/Dashboard/Alumnos/nuevo_ingreso.cshtml", lista);
	}
}