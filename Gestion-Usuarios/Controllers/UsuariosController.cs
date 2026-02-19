using System;
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
	public class UsuariosController : Controller
	{
		private readonly ContextDb _context;

		public UsuariosController(ContextDb context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			return View("~/Views/Dashboard/Usuarios/Index.cshtml");
		}

		[HttpGet]
		public async Task<IActionResult> GetUsersJson()
		{
			var listaUsuarios = new List<UsuarioViewModel>();

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

				using var rdr = await cmd.ExecuteReaderAsync();
				while (await rdr.ReadAsync())
				{
					var user = new UsuarioViewModel();

					user.Id = rdr["management_user_ID"] != DBNull.Value ? Convert.ToInt32(rdr["management_user_ID"]) : 0;

					string nombre = rdr["management_person_FirstName"] != DBNull.Value ? rdr["management_person_FirstName"].ToString() : string.Empty;
					string paterno = rdr["management_person_LastNamePaternal"] != DBNull.Value ? rdr["management_person_LastNamePaternal"].ToString() : string.Empty;
					string materno = rdr["management_person_LastNameMaternal"] != DBNull.Value ? rdr["management_person_LastNameMaternal"].ToString() : string.Empty;
					user.NombreCompleto = $"{nombre} {paterno} {materno}".Trim();

					user.Correo = rdr["management_user_Email"] != DBNull.Value ? rdr["management_user_Email"].ToString() : string.Empty;
					user.Roles = rdr["Roles"] != DBNull.Value ? rdr["Roles"].ToString() : "Sin Rol";

					string matricula = rdr["management_student_Matricula"] != DBNull.Value ? rdr["management_student_Matricula"].ToString() : string.Empty;
					string folio = rdr["management_student_EnrollmentFolio"] != DBNull.Value ? rdr["management_student_EnrollmentFolio"].ToString() : string.Empty;
					string empleado = rdr["management_teacher_EmployeeNumber"] != DBNull.Value ? rdr["management_teacher_EmployeeNumber"].ToString() : string.Empty;

					if (!string.IsNullOrEmpty(matricula))
					{
						user.Identificador = matricula;
						user.TipoUsuario = "Estudiante";
					}
					else if (!string.IsNullOrEmpty(folio))
					{
						user.Identificador = folio + " (F)";
						user.TipoUsuario = "Aspirante";
					}
					else if (!string.IsNullOrEmpty(empleado))
					{
						user.Identificador = empleado;
						user.TipoUsuario = "Docente";
					}
					else
					{
						user.Identificador = "N/A";
						user.TipoUsuario = "Admin";
					}

					user.Carrera = rdr["student_career"] != DBNull.Value ? rdr["student_career"].ToString() : "-";

					string grado = rdr["student_grado"] != DBNull.Value ? rdr["student_grado"].ToString() + "°" : string.Empty;
					string grupo = rdr["student_group"] != DBNull.Value ? rdr["student_group"].ToString() : string.Empty;
					user.Grupo = $"{grado} {grupo}".Trim();

					string statusAlumno = rdr["student_statuscode"] != DBNull.Value ? rdr["student_statuscode"].ToString() : string.Empty;
					bool userStatus = rdr["management_user_status"] != DBNull.Value && Convert.ToBoolean(rdr["management_user_status"]);

					user.Estado = !string.IsNullOrEmpty(statusAlumno) ? statusAlumno : (userStatus ? "ACTIVO" : "INACTIVO");

					listaUsuarios.Add(user);
				}
			}
			finally
			{
				await conn.CloseAsync();
			}

			return Json(new { data = listaUsuarios });
		}
	}
}