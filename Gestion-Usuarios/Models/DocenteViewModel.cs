using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Models
{
	public class DocenteViewModel
	{
		// Datos de Usuario
		public int management_user_ID { get; set; }
		public string management_user_Email { get; set; }

		// Datos de Persona
		public string management_person_FirstName { get; set; }
		public string management_person_LastNamePaternal { get; set; }
		public string management_person_LastNameMaternal { get; set; }
		public string management_person_Phone { get; set; }

		// Datos Específicos de Docente (Teacher)
		public int? teacher_ID { get; set; } // Puede ser null si el usuario no es docente
		public string management_teacher_EmployeeNumber { get; set; }
		public string teacher_statuscode { get; set; } // ACTIVO, SUSPENDIDO, etc.

		// Propiedad extra para mostrar nombre completo fácilmente
		public string NombreCompleto => $"{management_person_FirstName} {management_person_LastNamePaternal} {management_person_LastNameMaternal}";
	}
}
