using System.Linq;
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
		// El SP getview_student_full no filtra por @Student_IsFolio, así que filtramos aquí.
		var lista = await _repo.ExecuteStoredProcedureAsync(
			"getview_student_full",
			null,
			ModelMappers.MapToStudent
		);

		var nuevoIngreso = lista
			.Where(s => string.IsNullOrWhiteSpace(s.Matricula) && !string.IsNullOrWhiteSpace(s.Folio))
			.OrderBy(s => s.ApellidoPaterno)
			.ThenBy(s => s.ApellidoMaterno)
			.ThenBy(s => s.Nombres)
			.ToList();

		return View("~/Views/Dashboard/Alumnos/nuevo_ingreso.cshtml", nuevoIngreso);
	}
}
