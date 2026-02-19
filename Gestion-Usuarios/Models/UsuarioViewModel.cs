using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Models
{
	public class UsuarioViewModel
	{
		public int Id { get; set; }
		public string Identificador { get; set; } // Matrícula o Folio o Num Empleado
		public string NombreCompleto { get; set; }
		public string Correo { get; set; }
		public string Roles { get; set; } // Alumno, Admin, Docente
		public string Carrera { get; set; }
		public string Grupo { get; set; }
		public string Estado { get; set; } // Activo, Baja, etc.
		public string TipoUsuario { get; set; } // Para lógica de colores (Estudiante, Docente)
	}
}
