using System.Data.Common;
using Gestion_Usuarios.Data;

namespace Gestion_Usuarios.Models
{
	public static class ModelMappers
	{
		// 1. Mapeador para Alumnos, Bajas y Nuevo Ingreso
		public static StudentViewModel MapToStudent(DbDataReader reader)
		{
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
				EsActivo = ManagementRepository.GetValue<bool>(reader, "management_student_status")
			};
		}

		public static GroupViewModel MapToGroup(DbDataReader reader)
		{
			return new GroupViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_group_ID"),
				Nombre = ManagementRepository.GetValue<string>(reader, "management_group_Name") ?? "",
				EstadoCodigo = ManagementRepository.GetValue<string>(reader, "management_group_StatusCode") ?? "",
				EsActivo = ManagementRepository.GetValue<bool>(reader, "management_group_status")
			};
		}

		public static HistoricoViewModel MapToHistorico(DbDataReader reader)
		{
			return new HistoricoViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_user_ID"),
				Usuario = ManagementRepository.GetValue<string>(reader, "management_user_Username") ?? "",
				Email = ManagementRepository.GetValue<string>(reader, "management_user_Email") ?? "",
				NombreCompleto = ManagementRepository.GetValue<string>(reader, "FullName") ?? "",
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
				Email = ManagementRepository.GetValue<string>(reader, "management_user_Email") ?? "",
				Nombre = ManagementRepository.GetValue<string>(reader, "management_person_FirstName") ?? "",
				ApellidoPaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNamePaternal") ?? "",
				ApellidoMaterno = ManagementRepository.GetValue<string>(reader, "management_person_LastNameMaternal") ?? "",
				Telefono = ManagementRepository.GetValue<string>(reader, "management_person_Phone") ?? "",
				TeacherId = ManagementRepository.GetValue<int?>(reader, "teacher_ID"),
				NumeroEmpleado = ManagementRepository.GetValue<string>(reader, "management_teacher_EmployeeNumber") ?? "",
				Estado = ManagementRepository.GetValue<string>(reader, "teacher_statuscode") ?? "INACTIVO"
			};
		}

		// 3. Mapeador para la vista general de Usuarios (Administración)
		public static UsuarioViewModel MapToUsuario(DbDataReader reader)
		{
			var user = new UsuarioViewModel
			{
				Id = ManagementRepository.GetValue<int>(reader, "management_user_ID"),
				Correo = ManagementRepository.GetValue<string>(reader, "management_user_Email") ?? "",
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
			user.Grupo = ManagementRepository.GetValue<string>(reader, "group_Name")
				?? ManagementRepository.GetValue<string>(reader, "management_group_Name")
				?? ManagementRepository.GetValue<string>(reader, "group_GroupName")
				?? (ManagementRepository.GetValue<int?>(reader, "group_Grade")?.ToString())
				?? "-";

			// Estado: intentamos varios códigos/flags
			user.Estado = ManagementRepository.GetValue<string>(reader, "management_user_StatusCode")
				?? ManagementRepository.GetValue<string>(reader, "management_user_status")
				?? ManagementRepository.GetValue<string>(reader, "management_student_StatusCode")
				?? "INACTIVO";

			// Lógica de Identificador (Matrícula > Folio > Empleado)
			string matricula = ManagementRepository.GetValue<string>(reader, "management_student_Matricula") ?? "";
			string folio = ManagementRepository.GetValue<string>(reader, "management_student_EnrollmentFolio") ?? "";
			string empleado = ManagementRepository.GetValue<string>(reader, "management_teacher_EmployeeNumber") ?? "";

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

			return user;
		}
	}
}