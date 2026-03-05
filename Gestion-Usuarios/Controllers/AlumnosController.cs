using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gestion_Usuarios.Models;
using Gestion_Usuarios.Data;

namespace Gestion_Usuarios.Controllers
{
	[Authorize]
	public class AlumnosController : Controller
	{
		private readonly ManagementRepository _repo;

		public AlumnosController(ContextDb context)
		{
			_repo = new ManagementRepository(context);
		}

		// GET: /Alumnos
		public async Task<IActionResult> Index()
		{
			// Usamos el Repo + el Mapeador Global
			var lista = await _repo.ExecuteStoredProcedureAsync(
				"getview_student_full",
				null,
				ModelMappers.MapToStudent
			);

			return View("~/Views/Dashboard/Alumnos/Index.cshtml", lista);
		}

		// POST: /Alumnos/Delete/5
		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			if (id <= 0) return BadRequest(new { success = false });

			await _repo.ExecuteNonQueryAsync("management_student_softdelete", new Dictionary<string, object> {
				{ "@ID", id }
			});

			return Ok(new { success = true });
		}
	}
}