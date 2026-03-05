using Gestion_Usuarios.Data;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class UsuariosController : Controller
{
	private readonly ManagementRepository _repo;
	public UsuariosController(ContextDb context) => _repo = new ManagementRepository(context);

	public IActionResult Index() => View("~/Views/Dashboard/Usuarios/Index.cshtml");

	[HttpGet]
	public async Task<IActionResult> GetUsersJson()
	{
		var lista = await _repo.ExecuteStoredProcedureAsync(
			"getview_user_full",
			null,
			ModelMappers.MapToUsuario
		);

		return Json(new { data = lista });
	}
}