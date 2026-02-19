using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_Usuarios.Models;

namespace Gestion_Usuarios.Controllers
{
	[Authorize]
	public class DocentesController : Controller
	{
		private readonly ContextDb _context;

		public DocentesController(ContextDb context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			List<DocenteViewModel> listaDocentes = new List<DocenteViewModel>();

			var conn = _context.Database.GetDbConnection();
			await conn.OpenAsync();

			try
			{
				using var cmd = conn.CreateCommand();
				cmd.CommandText = "dbo.sp_management";
				cmd.CommandType = CommandType.StoredProcedure;

				var paramOption = cmd.CreateParameter();
				paramOption.ParameterName = "@Option";
				paramOption.Value = "getview_user_full";
				cmd.Parameters.Add(paramOption);

				using var dr = await cmd.ExecuteReaderAsync();
				while (await dr.ReadAsync())
				{
					var item = new DocenteViewModel();

					item.management_user_ID = dr["management_user_ID"] != DBNull.Value ? Convert.ToInt32(dr["management_user_ID"]) : 0;
					item.management_user_Email = dr["management_user_Email"] != DBNull.Value ? dr["management_user_Email"].ToString() : string.Empty;
					item.management_person_FirstName = dr["management_person_FirstName"] != DBNull.Value ? dr["management_person_FirstName"].ToString() : string.Empty;
					item.management_person_LastNamePaternal = dr["management_person_LastNamePaternal"] != DBNull.Value ? dr["management_person_LastNamePaternal"].ToString() : string.Empty;
					item.management_person_LastNameMaternal = dr["management_person_LastNameMaternal"] != DBNull.Value ? dr["management_person_LastNameMaternal"].ToString() : string.Empty;
					item.management_person_Phone = dr["management_person_Phone"] != DBNull.Value ? dr["management_person_Phone"].ToString() : string.Empty;

					// Datos de Teacher
					if (dr["teacher_ID"] != DBNull.Value)
					{
						item.teacher_ID = Convert.ToInt32(dr["teacher_ID"]);
						item.management_teacher_EmployeeNumber = dr["management_teacher_EmployeeNumber"] != DBNull.Value ? dr["management_teacher_EmployeeNumber"].ToString() : string.Empty;
						item.teacher_statuscode = dr["teacher_statuscode"] != DBNull.Value ? dr["teacher_statuscode"].ToString() : string.Empty;

						// SOLO agregamos a la lista si tiene ID de docente
						listaDocentes.Add(item);
					}
				}
			}
			finally
			{
				await conn.CloseAsync();
			}

			return View("~/Views/Dashboard/Docentes/Index.cshtml", listaDocentes);
		}
	}
}