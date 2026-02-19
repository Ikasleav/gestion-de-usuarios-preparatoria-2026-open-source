using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_Usuarios.Controllers
{
	[Authorize]
	public class AlumnosController : Controller
	{
		private readonly ContextDb _context;

		public AlumnosController(ContextDb context)
		{
			_context = context;
		}

        // POST: /Alumnos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentViewModel model)
        {
            if (id != model.Id) return BadRequest();

            // Simple update using stored procedure
            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "dbo.sp_management";
                cmd.CommandType = CommandType.StoredProcedure;

                var paramOption = cmd.CreateParameter();
                paramOption.ParameterName = "@Option";
                paramOption.Value = "management_student_update";
                cmd.Parameters.Add(paramOption);

                var paramId = cmd.CreateParameter();
                paramId.ParameterName = "@ID";
                paramId.Value = model.Id;
                cmd.Parameters.Add(paramId);

                var paramCareer = cmd.CreateParameter();
                paramCareer.ParameterName = "@Career";
                paramCareer.Value = model.Carrera ?? (object)DBNull.Value;
                cmd.Parameters.Add(paramCareer);

                var paramSem = cmd.CreateParameter();
                paramSem.ParameterName = "@Semestre";
                paramSem.Value = model.Semestre.HasValue ? model.Semestre.Value : (object)DBNull.Value;
                cmd.Parameters.Add(paramSem);

                await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                await conn.CloseAsync();
            }

            return RedirectToAction(nameof(Index));
        }

		// GET: /Alumnos
		public async Task<IActionResult> Index()
		{
			var lista = new List<StudentViewModel>();

			var conn = _context.Database.GetDbConnection();
			await conn.OpenAsync();

			try
			{
				using var cmd = conn.CreateCommand();

				// Configuración para usar el Stored Procedure
				cmd.CommandText = "dbo.sp_management";
				cmd.CommandType = CommandType.StoredProcedure;

				// Parámetro para seleccionar la vista de alumnos completa
				var paramOption = cmd.CreateParameter();
				paramOption.ParameterName = "@Option";
				paramOption.Value = "getview_student_full";
				cmd.Parameters.Add(paramOption);

				// Opcional: Si quisieras filtrar solo activos podrías agregar:
				// var paramStatus = cmd.CreateParameter();
				// paramStatus.ParameterName = "@Status";
				// paramStatus.Value = 1; 
				// cmd.Parameters.Add(paramStatus);

				using var reader = await cmd.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					var vm = new StudentViewModel
					{
						// Mapeo directo de las columnas que devuelve 'getview_student_full'
						Id = reader["management_student_ID"] != DBNull.Value ? Convert.ToInt32(reader["management_student_ID"]) : 0,

						Matricula = reader["management_student_Matricula"] != DBNull.Value ? reader["management_student_Matricula"].ToString() : null,

						Folio = reader["management_student_EnrollmentFolio"] != DBNull.Value ? reader["management_student_EnrollmentFolio"].ToString() : null,

						Nombres = reader["management_person_FirstName"] != DBNull.Value ? reader["management_person_FirstName"].ToString() : string.Empty,

						ApellidoPaterno = reader["management_person_LastNamePaternal"] != DBNull.Value ? reader["management_person_LastNamePaternal"].ToString() : string.Empty,

						ApellidoMaterno = reader["management_person_LastNameMaternal"] != DBNull.Value ? reader["management_person_LastNameMaternal"].ToString() : null,

						Carrera = reader["management_career_Name"] != DBNull.Value ? reader["management_career_Name"].ToString() : "Sin Asignar",

						Semestre = reader["Grado"] != DBNull.Value ? Convert.ToInt32(reader["Grado"]) : null,

						EstadoCodigo = reader["management_student_StatusCode"] != DBNull.Value ? reader["management_student_StatusCode"].ToString() : string.Empty,

						EsActivo = reader["management_student_status"] != DBNull.Value && Convert.ToBoolean(reader["management_student_status"])
					};

					lista.Add(vm);
				}
			}
			finally
			{
				await conn.CloseAsync();
			}

			return View("~/Views/Dashboard/Alumnos/Index.cshtml", lista);
		}

        // GET: /Alumnos/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return BadRequest();

            // Intentar obtener el alumno por Id usando el mismo SP (puedes cambiar por EF si prefieres)
            StudentViewModel vm = null;

            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "dbo.sp_management";
                cmd.CommandType = CommandType.StoredProcedure;

                var paramOption = cmd.CreateParameter();
                paramOption.ParameterName = "@Option";
                paramOption.Value = "getview_student_full";
                cmd.Parameters.Add(paramOption);

                var paramId = cmd.CreateParameter();
                paramId.ParameterName = "@ID";
                paramId.Value = id;
                cmd.Parameters.Add(paramId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    vm = new StudentViewModel
                    {
                        Id = reader["management_student_ID"] != DBNull.Value ? Convert.ToInt32(reader["management_student_ID"]) : 0,
                        Matricula = reader["management_student_Matricula"] != DBNull.Value ? reader["management_student_Matricula"].ToString() : null,
                        Folio = reader["management_student_EnrollmentFolio"] != DBNull.Value ? reader["management_student_EnrollmentFolio"].ToString() : null,
                        Nombres = reader["management_person_FirstName"] != DBNull.Value ? reader["management_person_FirstName"].ToString() : string.Empty,
                        ApellidoPaterno = reader["management_person_LastNamePaternal"] != DBNull.Value ? reader["management_person_LastNamePaternal"].ToString() : string.Empty,
                        ApellidoMaterno = reader["management_person_LastNameMaternal"] != DBNull.Value ? reader["management_person_LastNameMaternal"].ToString() : null,
                        Carrera = reader["management_career_Name"] != DBNull.Value ? reader["management_career_Name"].ToString() : "Sin Asignar",
                        Semestre = reader["Grado"] != DBNull.Value ? Convert.ToInt32(reader["Grado"]) : null,
                        EstadoCodigo = reader["management_student_StatusCode"] != DBNull.Value ? reader["management_student_StatusCode"].ToString() : string.Empty,
                        EsActivo = reader["management_student_status"] != DBNull.Value && Convert.ToBoolean(reader["management_student_status"])
                    };
                }
            }
            finally
            {
                await conn.CloseAsync();
            }

            if (vm == null) return NotFound();

            return View("~/Views/Dashboard/Alumnos/Edit.cshtml", vm);
        }

		// POST: /Alumnos/Delete/5
		[HttpPost]
		// [ValidateAntiForgeryToken] 
		public async Task<IActionResult> Delete(int id)
		{
			if (id <= 0) return BadRequest(new { success = false, message = "Id inválido" });

			var conn = _context.Database.GetDbConnection();
			await conn.OpenAsync();

			try
			{
				using var cmd = conn.CreateCommand();

				cmd.CommandText = "dbo.sp_management";
				cmd.CommandType = CommandType.StoredProcedure;

				var paramOption = cmd.CreateParameter();
				paramOption.ParameterName = "@Option";
				paramOption.Value = "management_student_softdelete";
				cmd.Parameters.Add(paramOption);

				var paramId = cmd.CreateParameter();
				paramId.ParameterName = "@ID";
				paramId.Value = id;
				cmd.Parameters.Add(paramId);

				// El SP también acepta @Student_StatusCode si quisieras cambiar el estado a algo específico como 'BAJA'
				// Por defecto el SP pone el status en 0.

				var affected = await cmd.ExecuteNonQueryAsync();

				// Nota: ExecuteNonQuery con SP a veces devuelve -1 si hay 'SET NOCOUNT ON'.
				// Si tu SP tiene 'SET NOCOUNT ON', mejor verifica lógica de retorno o asume éxito si no hay excepción.
				// En tu script veo que devuelve "SELECT @@ROWCOUNT", así que podrías usar ExecuteScalarAsync si quisieras el número exacto,
				// pero para un delete simple ExecuteNonQuery suele bastar si no validas estrictamente > 0.

				return Ok(new { success = true });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = ex.Message });
			}
			finally
			{
				await conn.CloseAsync();
			}
		}
	}
}