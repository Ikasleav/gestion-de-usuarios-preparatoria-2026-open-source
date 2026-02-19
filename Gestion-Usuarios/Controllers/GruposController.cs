using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Gestion_Usuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_Usuarios.Controllers
{
	[Authorize]
	public class GruposController : Controller
	{
		private readonly ContextDb _context;

		public GruposController(ContextDb context)
		{
			_context = context;
		}

		// GET: /Grupos
		public async Task<IActionResult> Index()
		{
			var lista = new List<GroupViewModel>();

			var conn = _context.Database.GetDbConnection();
			await conn.OpenAsync();

			try
			{
				using var cmd = conn.CreateCommand();
				cmd.CommandText = "dbo.sp_management";
				cmd.CommandType = CommandType.StoredProcedure;

				var paramOption = cmd.CreateParameter();
				paramOption.ParameterName = "@Option";
				paramOption.Value = "management_group_get";
				cmd.Parameters.Add(paramOption);

				using var reader = await cmd.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					var vm = new GroupViewModel
					{
						Id = HasColumn(reader, "management_group_ID") && reader["management_group_ID"] != DBNull.Value ? Convert.ToInt32(reader["management_group_ID"]) : 0,
						Nombre = HasColumn(reader, "management_group_Name") && reader["management_group_Name"] != DBNull.Value ? reader["management_group_Name"].ToString() : string.Empty,
						EstadoCodigo = HasColumn(reader, "management_group_StatusCode") && reader["management_group_StatusCode"] != DBNull.Value ? reader["management_group_StatusCode"].ToString() : string.Empty,
						EsActivo = HasColumn(reader, "management_group_status") && reader["management_group_status"] != DBNull.Value && Convert.ToBoolean(reader["management_group_status"])
					};

					lista.Add(vm);
				}
			}
			finally
			{
				await conn.CloseAsync();
			}

			return View("~/Views/Dashboard/Grupos/Index.cshtml", lista);
		}

		// GET: /Grupos/GetGrupos  (for DataTables / AJAX)
		[HttpGet]
		public async Task<JsonResult> GetGrupos()
		{
			var lista = new List<GroupViewModel>();

			var conn = _context.Database.GetDbConnection();
			await conn.OpenAsync();

			try
			{
				using var cmd = conn.CreateCommand();
				cmd.CommandText = "dbo.sp_management";
				cmd.CommandType = CommandType.StoredProcedure;

				var paramOption = cmd.CreateParameter();
				paramOption.ParameterName = "@Option";
				paramOption.Value = "management_group_get";
				cmd.Parameters.Add(paramOption);

				using var reader = await cmd.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					lista.Add(new GroupViewModel
					{
						Id = HasColumn(reader, "management_group_ID") && reader["management_group_ID"] != DBNull.Value ? Convert.ToInt32(reader["management_group_ID"]) : 0,
						Nombre = HasColumn(reader, "management_group_Name") && reader["management_group_Name"] != DBNull.Value ? reader["management_group_Name"].ToString() : string.Empty,
						EstadoCodigo = HasColumn(reader, "management_group_StatusCode") && reader["management_group_StatusCode"] != DBNull.Value ? reader["management_group_StatusCode"].ToString() : string.Empty,
						EsActivo = HasColumn(reader, "management_group_status") && reader["management_group_status"] != DBNull.Value && Convert.ToBoolean(reader["management_group_status"])
					});
				}
			}
			finally
			{
				await conn.CloseAsync();
			}

			return Json(new { data = lista });
		}

		// POST: /Grupos/Delete/5
		[HttpPost]
		[ValidateAntiForgeryToken]
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
				paramOption.Value = "management_group_softdelete";
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

		#region ViewModels
		private class GroupViewModel
		{
			public int Id { get; set; }
			public string Nombre { get; set; }
			public string EstadoCodigo { get; set; }
			public bool EsActivo { get; set; }
		}
		#endregion

		// Helper to avoid IndexOutOfRange when a column is not returned by the stored procedure
		private static bool HasColumn(IDataRecord reader, string columnName)
		{
			for (int i = 0; i < reader.FieldCount; i++)
			{
				if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}
	}
}