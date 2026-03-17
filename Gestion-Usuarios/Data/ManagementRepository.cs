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
		private const string StudentDashboardQuery = @"
SELECT
    s.management_student_ID,
    s.management_student_status,
    s.management_student_StatusCode,
    s.management_student_IsFolio,
    s.management_student_EnrollmentFolio,
    s.management_student_Matricula,
    s.management_student_createdDate,
    p.management_person_ID,
    p.management_person_FirstName,
    p.management_person_LastNamePaternal,
    p.management_person_LastNameMaternal,
    p.management_person_CURP,
    p.management_person_Email,
    p.management_person_Phone,
    c.management_career_ID,
    c.management_career_Code,
    c.management_career_Name,
    g.management_group_ID,
    g.management_group_Code,
    g.management_group_Name,
    g.management_group_Shift,
    TRY_CAST(LEFT(g.management_group_Code, 1) AS INT) AS Grado
FROM dbo.management_student_table s
INNER JOIN dbo.management_person_table p
    ON p.management_person_ID = s.management_student_PersonID
LEFT JOIN dbo.management_career_table c
    ON c.management_career_ID = s.management_student_CareerID
LEFT JOIN dbo.management_group_table g
    ON g.management_group_ID = s.management_student_GroupID
WHERE (@ID IS NULL OR s.management_student_ID = @ID)
  AND (@Status IS NULL OR s.management_student_status = @Status)
  AND (@StudentCareerID IS NULL OR s.management_student_CareerID = @StudentCareerID)
  AND (@StudentGroupID IS NULL OR s.management_student_GroupID = @StudentGroupID)
  AND (@Student_IsFolio IS NULL OR s.management_student_IsFolio = @Student_IsFolio)
ORDER BY s.management_student_ID DESC;";

		private const string UserDashboardQuery = @"
;WITH RolesAgg AS
(
    SELECT
        ur.management_userrole_UserID AS UserID,
        STRING_AGG(r.management_role_Name, ', ') WITHIN GROUP (ORDER BY r.management_role_Name) AS Roles
    FROM dbo.management_userrole_table ur
    INNER JOIN dbo.management_role_table r
        ON r.management_role_ID = ur.management_userrole_RoleID
    WHERE ur.management_userrole_status = 1
      AND r.management_role_status = 1
    GROUP BY ur.management_userrole_UserID
)
SELECT
    u.management_user_ID,
    u.management_user_status,
    u.management_user_Username,
    u.management_user_Email,
    u.management_user_IsLocked,
    u.management_user_LockReason,
    u.management_user_LastLoginDate,
    u.management_user_createdDate,
    p.management_person_ID,
    p.management_person_FirstName,
    p.management_person_LastNamePaternal,
    p.management_person_LastNameMaternal,
    p.management_person_CURP,
    p.management_person_Email AS person_email,
    p.management_person_Phone,
    ra.Roles,
    s.management_student_ID AS student_ID,
    s.management_student_status AS student_status,
    s.management_student_StatusCode AS student_statuscode,
    s.management_student_IsFolio,
    s.management_student_EnrollmentFolio,
    s.management_student_Matricula,
    c.management_career_Name AS student_career,
    g.management_group_Code AS student_group,
    TRY_CAST(LEFT(g.management_group_Code, 1) AS INT) AS student_grado,
    t.management_teacher_ID AS teacher_ID,
    t.management_teacher_status AS teacher_status,
    t.management_teacher_EmployeeNumber,
    t.management_teacher_StatusCode AS teacher_statuscode
FROM dbo.management_user_table u
INNER JOIN dbo.management_person_table p
    ON p.management_person_ID = u.management_user_PersonID
LEFT JOIN RolesAgg ra
    ON ra.UserID = u.management_user_ID
LEFT JOIN dbo.management_student_table s
    ON s.management_student_PersonID = p.management_person_ID
LEFT JOIN dbo.management_career_table c
    ON c.management_career_ID = s.management_student_CareerID
LEFT JOIN dbo.management_group_table g
    ON g.management_group_ID = s.management_student_GroupID
LEFT JOIN dbo.management_teacher_table t
    ON t.management_teacher_PersonID = p.management_person_ID
WHERE (@ID IS NULL OR u.management_user_ID = @ID)
  AND (@Status IS NULL OR u.management_user_status = @Status)
  AND (@UserPersonID IS NULL OR u.management_user_PersonID = @UserPersonID)
  AND (@Username IS NULL OR u.management_user_Username = @Username)
  AND (@UserEmail IS NULL OR u.management_user_Email = @UserEmail)
ORDER BY u.management_user_ID DESC;";

		private const string GroupDashboardQuery = @"
SELECT
    g.management_group_ID,
    g.management_group_CareerID,
    g.management_group_Code,
    g.management_group_Name,
    g.management_group_Shift,
    g.management_group_status,
    g.management_group_createdDate,
    c.management_career_Name
FROM dbo.management_group_table g
LEFT JOIN dbo.management_career_table c
    ON c.management_career_ID = g.management_group_CareerID
WHERE (@ID IS NULL OR g.management_group_ID = @ID)
  AND (@Status IS NULL OR g.management_group_status = @Status)
  AND (@GroupCareerID IS NULL OR g.management_group_CareerID = @GroupCareerID)
  AND (@GroupCode IS NULL OR g.management_group_Code = @GroupCode)
ORDER BY g.management_group_ID DESC;";

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
			if (TryGetDashboardQuery(option, parameters, out var sql, out var directParameters))
			{
				return await ExecuteQueryAsync(sql, directParameters, mapFunction);
			}

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

		public async Task<List<T>> ExecuteQueryAsync<T>(
			string sql,
			Dictionary<string, object>? parameters,
			Func<DbDataReader, T> mapFunction)
		{
			var results = new List<T>();
			var conn = _context.Database.GetDbConnection();

			using var cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			cmd.CommandType = CommandType.Text;

			if (parameters != null)
			{
				foreach (var param in parameters)
				{
					cmd.Parameters.Add(new SqlParameter(param.Key, param.Value ?? DBNull.Value));
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

		public async Task<int> ExecuteCommandAsync(string sql, Dictionary<string, object>? parameters)
		{
			var conn = _context.Database.GetDbConnection();

			using var cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			cmd.CommandType = CommandType.Text;

			if (parameters != null)
			{
				foreach (var param in parameters)
				{
					cmd.Parameters.Add(new SqlParameter(param.Key, param.Value ?? DBNull.Value));
				}
			}

			if (conn.State != ConnectionState.Open) await conn.OpenAsync();

			try
			{
				return await cmd.ExecuteNonQueryAsync();
			}
			finally
			{
				await conn.CloseAsync();
			}
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

		private static bool TryGetDashboardQuery(
			string option,
			Dictionary<string, object>? parameters,
			out string sql,
			out Dictionary<string, object> directParameters)
		{
			sql = string.Empty;
			directParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			switch (option)
			{
				case "getview_student_full":
					sql = StudentDashboardQuery;
					directParameters["@ID"] = GetParameterValue(parameters, "@ID");
					directParameters["@Status"] = GetParameterValue(parameters, "@Status");
					directParameters["@StudentCareerID"] = GetParameterValue(parameters, "@StudentCareerID");
					directParameters["@StudentGroupID"] = GetParameterValue(parameters, "@StudentGroupID");
					directParameters["@Student_IsFolio"] = GetParameterValue(parameters, "@Student_IsFolio");
					return true;

				case "getview_user_full":
					sql = UserDashboardQuery;
					directParameters["@ID"] = GetParameterValue(parameters, "@ID");
					directParameters["@Status"] = GetParameterValue(parameters, "@Status");
					directParameters["@UserPersonID"] = GetParameterValue(parameters, "@UserPersonID");
					directParameters["@Username"] = GetParameterValue(parameters, "@Username");
					directParameters["@UserEmail"] = GetParameterValue(parameters, "@UserEmail");
					return true;

				case "management_group_get":
					sql = GroupDashboardQuery;
					directParameters["@ID"] = GetParameterValue(parameters, "@ID");
					directParameters["@Status"] = GetParameterValue(parameters, "@Status");
					directParameters["@GroupCareerID"] = GetParameterValue(parameters, "@GroupCareerID");
					directParameters["@GroupCode"] = GetParameterValue(parameters, "@GroupCode");
					return true;

				default:
					return false;
			}
		}

		private static object GetParameterValue(Dictionary<string, object>? parameters, string key)
		{
			if (parameters != null && parameters.TryGetValue(key, out var value) && value != null)
			{
				return value;
			}

			return DBNull.Value;
		}
	}
}
