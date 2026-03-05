using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Gestion_Usuarios.Models;

namespace Gestion_Usuarios.Data
{
	public class ManagementRepository
	{
		private readonly DbContext _context;

		public ManagementRepository(ContextDb context)
		{
			_context = context;
		}

		/// Ejecuta el SP dbo.sp_management y mapea los resultados a una lista de objetos.
		public async Task<List<T>> ExecuteStoredProcedureAsync<T>(
			string option,
			Dictionary<string, object>? parameters,
			Func<DbDataReader, T> mapFunction)
		{
			var results = new List<T>();
			var conn = _context.Database.GetDbConnection();

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "dbo.sp_management";
			cmd.CommandType = CommandType.StoredProcedure;

			// Parámetro obligatorio
			var paramOption = new SqlParameter("@Option", option);
			cmd.Parameters.Add(paramOption);

			// Parámetros dinámicos
			if (parameters != null)
			{
				foreach (var param in parameters)
				{
					var sqlParam = new SqlParameter(param.Key, param.Value ?? DBNull.Value);
					cmd.Parameters.Add(sqlParam);
				}
			}

			if (conn.State != ConnectionState.Open) await conn.OpenAsync();

			try
			{
				using var reader = await cmd.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					results.Add(mapFunction(reader));
				}
			}
			finally
			{
				await conn.CloseAsync();
			}

			return results;
		}

		/// Ejecuta el SP para operaciones que no devuelven filas (Insert, Update, Delete).
		public async Task ExecuteNonQueryAsync(string option, Dictionary<string, object> parameters)
		{
			var conn = _context.Database.GetDbConnection();
			using var cmd = conn.CreateCommand();
			cmd.CommandText = "dbo.sp_management";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@Option", option));
			foreach (var param in parameters)
			{
				cmd.Parameters.Add(new SqlParameter(param.Key, param.Value ?? DBNull.Value));
			}

			if (conn.State != ConnectionState.Open) await conn.OpenAsync();
			try
			{
				await cmd.ExecuteNonQueryAsync();
			}
			finally
			{
				await conn.CloseAsync();
			}
		}

		// Helper estático para leer columnas de forma segura
		public static T? GetValue<T>(DbDataReader reader, string columnName)
		{
			try
			{
				int ordinal = reader.GetOrdinal(columnName);
				if (reader.IsDBNull(ordinal)) return default;

				object rawValue = reader.GetValue(ordinal);
				Type targetType = typeof(T);
				Type? underlying = Nullable.GetUnderlyingType(targetType);
				Type convertType = underlying ?? targetType;

				// If value already matches the desired convertType, try to return directly.
				if (convertType.IsInstanceOfType(rawValue))
				{
					// If target is nullable, create a boxed Nullable<T> instance
					if (underlying != null)
					{
						var nullableType = typeof(Nullable<>).MakeGenericType(underlying);
						var boxedNullable = Activator.CreateInstance(nullableType, rawValue);
						return (T)boxedNullable!;
					}

					return (T)rawValue;
				}

				// Convert to the non-nullable target type
				var converted = Convert.ChangeType(rawValue, convertType);

				if (underlying != null)
				{
					// Build boxed Nullable<T> with the converted value
					var nullableType = typeof(Nullable<>).MakeGenericType(underlying);
					var boxedNullable = Activator.CreateInstance(nullableType, converted);
					return (T)boxedNullable!;
				}

				return (T)converted!;
			}
			catch (IndexOutOfRangeException)
			{
				return default; // La columna no existe en el resultado del SP
			}
		}
	}
}