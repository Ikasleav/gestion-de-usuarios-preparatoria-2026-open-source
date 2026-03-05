namespace Gestion_Usuarios.Models
{
	public class DocenteViewModel
	{
		// Identificadores
		public int UserId { get; set; }
		public int? TeacherId { get; set; }

		// Datos de contacto
		public string Email { get; set; } = string.Empty;
		public string Telefono { get; set; } = string.Empty;

		// Datos personales
		public string Nombre { get; set; } = string.Empty;
		public string ApellidoPaterno { get; set; } = string.Empty;
		public string ApellidoMaterno { get; set; } = string.Empty;

		// Datos laborales
		public string NumeroEmpleado { get; set; } = string.Empty;
		public string Estado { get; set; } = string.Empty; // ACTIVO, SUSPENDIDO

		// Propiedad calculada
		public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();

		// Clase CSS para la vista
		public string BadgeClass => Estado.ToUpper() == "ACTIVO" ? "bg-success" : "bg-warning";
	}
}