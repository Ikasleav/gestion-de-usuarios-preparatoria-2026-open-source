using System;
using System.ComponentModel.DataAnnotations;

namespace Gestion_Usuarios.Models
{
	public class StudentViewModel
	{
		// ID para las acciones (Editar/Eliminar)
		// Mapea: s.management_student_ID
		public int Id { get; set; }

		// Mapea: s.management_student_Matricula
		public string? Matricula { get; set; }

		// Mapea: s.management_student_EnrollmentFolio
		// Útil si el alumno es PREINSCRITO y aún no tiene matrícula
		public string? Folio { get; set; }

		// Para mostrar en la tabla, prefiere Matrícula, si no existe, muestra Folio
		public string Identificador => !string.IsNullOrEmpty(Matricula) ? Matricula : Folio;

		// Datos Personales
		// Mapea: p.management_person_FirstName
		public string Nombres { get; set; } = string.Empty;

		// Mapea: p.management_person_LastNamePaternal
		public string ApellidoPaterno { get; set; } = string.Empty;

		// Mapea: p.management_person_LastNameMaternal
		public string? ApellidoMaterno { get; set; }

		// Propiedad calculada para la columna "Nombre Completo"
		public string NombreCompleto => $"{Nombres} {ApellidoPaterno} {ApellidoMaterno}".Trim();

		// Mapea: c.management_career_Name
		public string Carrera { get; set; } = "Sin Asignar";

		// Mapea: Grado (calculado en el SP desde management_group_Code)
		// Columna "Semestre" en la vista
		public int? Semestre { get; set; }

		// Mapea: s.management_student_StatusCode (ej. 'INSCRITO', 'PREINSCRITO', 'BAJA')
		public string EstadoCodigo { get; set; } = string.Empty;

		// Mapea: s.management_student_status (1 = Activo, 0 = Inactivo/Borrado lógico)
		public bool EsActivo { get; set; }

		// Helpers para las Badges de la vista (Colores)
		public string EstadoBadgeClass
		{
			get
			{
				if (!EsActivo) return "bg-danger"; // Baja lógica

				return EstadoCodigo.ToUpper() switch
				{
					"INSCRITO" => "bg-success",    // Activo
					"PREINSCRITO" => "bg-warning text-dark", // Pendiente
					"BAJA" => "bg-danger",
					_ => "bg-secondary"
				};
			}
		}
	}
}