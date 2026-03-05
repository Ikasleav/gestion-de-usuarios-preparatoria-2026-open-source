using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gestion_Usuarios.Models;
using Gestion_Usuarios.Data;

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
			// Obtenemos todos los usuarios con sus datos extendidos
			var todos = await _repo.ExecuteStoredProcedureAsync(
				"getview_user_full",
				null,
				ModelMappers.MapToDocente
			);

			// Filtramos solo los que realmente son docentes (tienen ID de teacher)
			var listaDocentes = todos.Where(d => d.TeacherId.HasValue).ToList();

			return View("~/Views/Dashboard/Docentes/Index.cshtml", listaDocentes);
		}
	}
}