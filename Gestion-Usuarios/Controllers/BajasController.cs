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
    public class BajasController : Controller
    {
        private readonly ContextDb _context;
        // Change this to the correct option your SP expects for bajas if different.
        private const string OptionGetView = "getview_student_full";

        public BajasController(ContextDb context)
        {
            _context = context;
        }

        // GET: /Bajas
        public async Task<IActionResult> Index()
        {
            var lista = new List<StudentViewModel>();

            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "dbo.sp_management";
                cmd.CommandType = CommandType.StoredProcedure;

                var paramOption = cmd.CreateParameter();
                paramOption.ParameterName = "@Option";
                paramOption.Value = OptionGetView;
                cmd.Parameters.Add(paramOption);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var vm = new StudentViewModel
                    {
                        Id = GetInt(reader, new[] { "management_student_ID", "student_ID" }),
                        Matricula = GetString(reader, new[] { "management_student_Matricula", "student_Matricula" }) 
                                   ?? GetString(reader, new[] { "management_student_EnrollmentFolio", "student_EnrollmentFolio" }),
                        Folio = GetString(reader, new[] { "management_student_EnrollmentFolio", "student_EnrollmentFolio" }),
                        Nombres = GetString(reader, new[] { "management_person_FirstName", "person_FirstName" }) ?? string.Empty,
                        ApellidoPaterno = GetString(reader, new[] { "management_person_LastNamePaternal", "person_LastNamePaternal" }) ?? string.Empty,
                        ApellidoMaterno = GetString(reader, new[] { "management_person_LastNameMaternal", "person_LastNameMaternal" }),
                        Carrera = GetString(reader, new[] { "management_career_Name", "career_Name" }) ?? "Sin Asignar",
                        Semestre = GetIntNullable(reader, new[] { "Grado", "group_Grade" }),
                        EstadoCodigo = GetString(reader, new[] { "management_student_StatusCode", "student_StatusCode" }),
                        EsActivo = GetBool(reader, new[] { "management_student_status", "student_status" })
                    };

                    lista.Add(vm);
                }
            }
            finally
            {
                await conn.CloseAsync();
            }

            return View("~/Views/Dashboard/Alumnos/bajas.cshtml", lista);
        }

        // GET: /Bajas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return BadRequest();

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
                paramOption.Value = OptionGetView;
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
                        Id = GetInt(reader, new[] { "management_student_ID", "student_ID" }),
                        Matricula = GetString(reader, new[] { "management_student_Matricula", "student_Matricula" }),
                        Folio = GetString(reader, new[] { "management_student_EnrollmentFolio", "student_EnrollmentFolio" }),
                        Nombres = GetString(reader, new[] { "management_person_FirstName", "person_FirstName" }) ?? string.Empty,
                        ApellidoPaterno = GetString(reader, new[] { "management_person_LastNamePaternal", "person_LastNamePaternal" }) ?? string.Empty,
                        ApellidoMaterno = GetString(reader, new[] { "management_person_LastNameMaternal", "person_LastNameMaternal" }),
                        Carrera = GetString(reader, new[] { "management_career_Name", "career_Name" }) ?? "Sin Asignar",
                        Semestre = GetIntNullable(reader, new[] { "Grado", "group_Grade" }),
                        EstadoCodigo = GetString(reader, new[] { "management_student_StatusCode", "student_StatusCode" }),
                        EsActivo = GetBool(reader, new[] { "management_student_status", "student_status" })
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

        // POST: /Bajas/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentViewModel model)
        {
            if (id != model.Id) return BadRequest();

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

        // POST: /Bajas/Delete/5
        [HttpPost]
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

                await cmd.ExecuteNonQueryAsync();

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

        #region Reader helpers
        private static string GetString(DbDataReader reader, string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                try
                {
                    var ord = reader.GetOrdinal(name);
                    if (!reader.IsDBNull(ord)) return reader.GetValue(ord)?.ToString();
                }
                catch (IndexOutOfRangeException) { /* column not present, continue */ }
            }
            return null;
        }

        private static int GetInt(DbDataReader reader, string[] candidateNames)
        {
            var val = GetIntNullable(reader, candidateNames);
            return val ?? 0;
        }

        private static int? GetIntNullable(DbDataReader reader, string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                try
                {
                    var ord = reader.GetOrdinal(name);
                    if (!reader.IsDBNull(ord)) return Convert.ToInt32(reader.GetValue(ord));
                }
                catch (IndexOutOfRangeException) { }
            }
            return null;
        }

        private static bool GetBool(DbDataReader reader, string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                try
                {
                    var ord = reader.GetOrdinal(name);
                    if (!reader.IsDBNull(ord)) return Convert.ToBoolean(reader.GetValue(ord));
                }
                catch (IndexOutOfRangeException) { }
            }
            return false;
        }
        #endregion
    }
}