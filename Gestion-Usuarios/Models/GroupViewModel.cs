namespace Gestion_Usuarios.Models
{
	public class GroupViewModel
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string EstadoCodigo { get; set; } = string.Empty;
		public bool EsActivo { get; set; }

		// Propiedad para la interfaz de usuario
		public string StatusColor => EsActivo ? "text-success" : "text-danger";
		public string StatusLabel => EsActivo ? "Activo" : "Inactivo";
	}
}