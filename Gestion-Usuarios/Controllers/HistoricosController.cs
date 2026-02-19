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
	public class HistoricosController : Controller
	{
		private readonly ContextDb _context;

		public HistoricosController(ContextDb context)
		{
			_context = context;
		}

		// GET: /Historicos
		public async Task<IActionResult> Index()
		{
			var lista = new List<HistoricoViewModel>();

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

				using var reader = await cmd.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					lista.Add(new HistoricoViewModel
					{
						Id = HasColumn(reader, "management_user_ID") && reader["management_user_ID"] != DBNull.Value ? Convert.ToInt32(reader["management_user_ID"]) : 0,
						Fecha = HasColumn(reader, "management_user_createdDate") && reader["management_user_createdDate"] != DBNull.Value ? Convert.ToDateTime(reader["management_user_createdDate"]).ToString("yyyy-MM-dd HH:mm") : string.Empty,
						Usuario = HasColumn(reader, "management_user_Username") && reader["management_user_Username"] != DBNull.Value ? reader["management_user_Username"].ToString() : string.Empty,
						Email = HasColumn(reader, "management_user_Email") && reader["management_user_Email"] != DBNull.Value ? reader["management_user_Email"].ToString() : string.Empty,
						Estado = HasColumn(reader, "management_user_status") && reader["management_user_status"] != DBNull.Value && Convert.ToBoolean(reader["management_user_status"]) ? "Activo" : "Inactivo",
						NombreCompleto = HasColumn(reader, "FullName") && reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : string.Empty
					});
				}
			}
			finally
			{
				await conn.CloseAsync();
			}

			return View("~/Views/Dashboard/Historicos/Index.cshtml", lista);
		}

		// GET: /Historicos/GetHistorial  (for DataTables / AJAX)
		[HttpGet]
		public async Task<JsonResult> GetHistorial()
		{
			var lista = new List<HistoricoViewModel>();

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

				using var reader = await cmd.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					lista.Add(new HistoricoViewModel
					{
						Id = HasColumn(reader, "management_user_ID") && reader["management_user_ID"] != DBNull.Value ? Convert.ToInt32(reader["management_user_ID"]) : 0,
						Fecha = HasColumn(reader, "management_user_createdDate") && reader["management_user_createdDate"] != DBNull.Value ? Convert.ToDateTime(reader["management_user_createdDate"]).ToString("yyyy-MM-dd HH:mm") : string.Empty,
						Usuario = HasColumn(reader, "management_user_Username") && reader["management_user_Username"] != DBNull.Value ? reader["management_user_Username"].ToString() : string.Empty,
						Email = HasColumn(reader, "management_user_Email") && reader["management_user_Email"] != DBNull.Value ? reader["management_user_Email"].ToString() : string.Empty,
						Estado = HasColumn(reader, "management_user_status") && reader["management_user_status"] != DBNull.Value && Convert.ToBoolean(reader["management_user_status"]) ? "Activo" : "Inactivo",
						NombreCompleto = HasColumn(reader, "FullName") && reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : string.Empty
					});
				}
			}
			finally
			{
				await conn.CloseAsync();
			}

			return Json(new { data = lista });
		}

		// POST: /Historicos/Delete/5
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
				paramOption.Value = "management_user_softdelete";
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
		private class HistoricoViewModel
		{
			public int Id { get; set; }
			public string Fecha { get; set; }
			public string Usuario { get; set; }
			public string Email { get; set; }
			public string Estado { get; set; }
			public string NombreCompleto { get; set; }
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