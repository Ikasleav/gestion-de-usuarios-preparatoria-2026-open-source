using Gestion_Usuarios.Data;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class BajasController : Controller
{
    private readonly ManagementRepository _repo;
    public BajasController(ContextDb context) => _repo = new ManagementRepository(context);

    public async Task<IActionResult> Index()
    {
        // Call the view SP without extra parameters (the stored procedure does not accept @Status here).
        var lista = await _repo.ExecuteStoredProcedureAsync(
            "getview_student_full",
            null,
            ModelMappers.MapToStudent
        );

        // Filter in-memory for bajas: either EsActivo == false or explicit status code "BAJA"
        var bajas = lista.Where(s =>
            !s.EsActivo ||
            (string.Equals(s.EstadoCodigo ?? string.Empty, "BAJA", System.StringComparison.OrdinalIgnoreCase))
        ).ToList();

        return View("~/Views/Dashboard/Alumnos/Bajas.cshtml", bajas);
    }
}