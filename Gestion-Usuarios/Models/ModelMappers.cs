using System.Data.Common;
using Gestion_Usuarios.Data;

namespace Gestion_Usuarios.Models
{
	public static class ModelMappers
	{
		// 1. Mapeador para Alumnos, Bajas y Nuevo Ingreso
		public static StudentViewModel MapToStudent(DbDataReader reader)
		{
			var groupCode = ManagementRepository.GetValue<string>(reader, "management_group_Code")
				?? ManagementRepository.GetValue<string>(reader, "student_group")
				?? ManagementRepository.GetValue<string>(reader, "group_Code");
			var groupName = ManagementRepository.GetValue<string>(reader, "management_group_Name")
				?? ManagementRepository.GetValue<string>(reader, "group_Name")
				?? ManagementRepository.GetValue<string>(reader, "group_GroupName");
			var groupDisplay = !string.IsNullOrWhiteSpace(groupCode) && !string.IsNullOrWhiteSpace(groupName)
				? $"{groupCode} - {groupName}"
				: groupCode ?? groupName ?? string.Empty;

			return new StudentViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_student_ID"),
				Matricula = ManagementRepository.GetValue<string>(reader, "management_student_Matricula")
							?? ManagementRepository.GetValue<string>(reader, "student_Matricula"),
				Folio = ManagementRepository.GetValue<string>(reader, "management_student_EnrollmentFolio"),
				Nombres = ManagementRepository.GetValue<string>(reader, "management_person_FirstName") ?? "",
				ApellidoPaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNamePaternal") ?? "",
				ApellidoMaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNameMaternal"),
				Carrera = ManagementRepository.GetValue<string>(reader, "management_career_Name") ?? "Sin Asignar",
				Semestre = ManagementRepository.GetValue<int?>(reader, "Grado")
						   ?? ManagementRepository.GetValue<int?>(reader, "group_Grade"),
				EstadoCodigo = ManagementRepository.GetValue<string>(reader, "management_student_StatusCode") ?? "",
				EsActivo = ManagementRepository.GetValue<bool>(reader, "management_student_status"),
				CURP = ManagementRepository.GetValue<string>(reader, "management_person_CURP")
					   ?? ManagementRepository.GetValue<string>(reader, "person_CURP")
					   ?? ManagementRepository.GetValue<string>(reader, "CURP"),
				Email = ManagementRepository.GetValue<string>(reader, "management_person_Email")
						?? ManagementRepository.GetValue<string>(reader, "person_email")
						?? ManagementRepository.GetValue<string>(reader, "management_user_Email")
						?? ManagementRepository.GetValue<string>(reader, "user_Email")
						?? ManagementRepository.GetValue<string>(reader, "Email"),
				Grupo = groupDisplay
			};
		}

		public static GroupViewModel MapToGroup(DbDataReader reader)
		{
			return new GroupViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_group_ID"),
				Carrera = ManagementRepository.GetValue<string>(reader, "management_career_Name") ?? "Sin carrera",
				Codigo = ManagementRepository.GetValue<string>(reader, "management_group_Code") ?? "",
				Nombre = ManagementRepository.GetValue<string>(reader, "management_group_Name") ?? "",
				Turno = ManagementRepository.GetValue<string>(reader, "management_group_Shift") ?? "",
				EsActivo = ManagementRepository.GetValue<bool>(reader, "management_group_status")
			};
		}

		public static HistoricoViewModel MapToHistorico(DbDataReader reader)
		{
			var nombreCompleto = (
				(ManagementRepository.GetValue<string>(reader, "management_person_FirstName") ?? "") + " " +
				(ManagementRepository.GetValue<string>(reader, "management_person_LastNamePaternal") ?? "") + " " +
				(ManagementRepository.GetValue<string>(reader, "management_person_LastNameMaternal") ?? "")
			).Trim();

			return new HistoricoViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_user_ID"),
				Usuario = ManagementRepository.GetValue<string>(reader, "management_user_Username") ?? "",
				Email = ManagementRepository.GetValue<string>(reader, "management_user_Email")
					?? ManagementRepository.GetValue<string>(reader, "person_email")
					?? "",
				NombreCompleto = nombreCompleto,
				EsActivo = ManagementRepository.GetValue<bool>(reader, "management_user_status"),
				FechaCreacion = ManagementRepository.GetValue<DateTime>(reader, "management_user_createdDate")
			};
		}

		// 2. Mapeador para Docentes
		public static DocenteViewModel MapToDocente(DbDataReader reader)
		{
			return new DocenteViewModel
			{
				UserId = ManagementRepository.GetValue<int>(reader, "management_user_ID"),
				Email = ManagementRepository.GetValue<string>(reader, "management_user_Email")
					?? ManagementRepository.GetValue<string>(reader, "person_email")
					?? "",
				Nombre = ManagementRepository.GetValue<string>(reader, "management_person_FirstName") ?? "",
				ApellidoPaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNamePaternal") ?? "",
				ApellidoMaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNameMaternal") ?? "",
				Telefono = ManagementRepository.GetValue<string>(reader, "management_person_Phone") ?? "",
				TeacherId = ManagementRepository.GetValue<int?>(reader, "teacher_ID"),
				NumeroEmpleado = ManagementRepository.GetValue<string>(reader, "management_teacher_EmployeeNumber") ?? "",
				Estado = ManagementRepository.GetValue<string>(reader, "teacher_statuscode")
					?? (ManagementRepository.GetValue<bool?>(reader, "teacher_status") == true ? "ACTIVO" : "INACTIVO")
			};
		}

		// 3. Mapeador para la vista general de Usuarios (Administración)
		public static UsuarioViewModel MapToUsuario(DbDataReader reader)
		{
			var user = new UsuarioViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_user_ID"),
				Correo = ManagementRepository.GetValue<string>(reader, "management_user_Email")
					?? ManagementRepository.GetValue<string>(reader, "person_email")
					?? "",
				// Roles en la BD puede venir como "Roles"
				Roles = ManagementRepository.GetValue<string>(reader, "Roles") ?? "Sin Rol",
				Carrera = ManagementRepository.GetValue<string>(reader, "student_career") ?? "-",
			};

			// Nombre completo: preferimos columna FullName, si no existe concatenamos
			user.NombreCompleto = ManagementRepository.GetValue<string>(reader, "FullName")
				?? ( (ManagementRepository.GetValue<string>(reader, "management_person_FirstName") ?? "") + " " +
					 (ManagementRepository.GetValue<string>(reader, "management_person_LastNamePaternal") ?? "") + " " +
					 (ManagementRepository.GetValue<string>(reader, "management_person_LastNameMaternal") ?? "") ).Trim();

			// Grupo: intentamos varias columnas comunes
			user.Grupo = ManagementRepository.GetValue<string>(reader, "student_group")
				?? ManagementRepository.GetValue<string>(reader, "group_Name")
				?? ManagementRepository.GetValue<string>(reader, "management_group_Name")
				?? ManagementRepository.GetValue<string>(reader, "management_group_Code")
				?? ManagementRepository.GetValue<string>(reader, "group_GroupName")
				?? (ManagementRepository.GetValue<int?>(reader, "group_Grade")?.ToString())
				?? "-";

			// Lógica de Identificador (Matrícula > Folio > Empleado)
			string matricula = ManagementRepository.GetValue<string>(reader, "management_student_Matricula") ?? "";
			string folio = ManagementRepository.GetValue<string>(reader, "management_student_EnrollmentFolio") ?? "";
			string empleado = ManagementRepository.GetValue<string>(reader, "management_teacher_EmployeeNumber") ?? "";
			bool userActivo = ManagementRepository.GetValue<bool?>(reader, "management_user_status") ?? false;
			bool? studentActivo = ManagementRepository.GetValue<bool?>(reader, "student_status");
			bool? teacherActivo = ManagementRepository.GetValue<bool?>(reader, "teacher_status");

			if (!string.IsNullOrEmpty(matricula))
			{
				user.Identificador = matricula;
				user.TipoUsuario = "Estudiante";
				user.Estado = ManagementRepository.GetValue<string>(reader, "student_statuscode")
					?? (studentActivo == true ? "ACTIVO" : "INACTIVO");
			}
			else if (!string.IsNullOrEmpty(folio))
			{
				user.Identificador = folio + " (F)";
				user.TipoUsuario = "Aspirante";
				user.Estado = ManagementRepository.GetValue<string>(reader, "student_statuscode")
					?? (studentActivo == true ? "PREINSCRITO" : "INACTIVO");
			}
			else if (!string.IsNullOrEmpty(empleado))
			{
				user.Identificador = empleado;
				user.TipoUsuario = "Docente";
				user.Estado = ManagementRepository.GetValue<string>(reader, "teacher_statuscode")
					?? (teacherActivo == true ? "ACTIVO" : "INACTIVO");
			}
			else
			{
				user.Identificador = "N/A";
				user.TipoUsuario = "Admin";
				user.Estado = userActivo ? "ACTIVO" : "INACTIVO";
			}

			return user;
		}
	}
}
