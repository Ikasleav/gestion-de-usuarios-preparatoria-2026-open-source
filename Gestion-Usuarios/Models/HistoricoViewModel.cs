using System;

namespace Gestion_Usuarios.Models
{
	public class HistoricoViewModel
	{
		public int Id { get; set; }
		public string Usuario { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string NombreCompleto { get; set; } = string.Empty;
		public bool EsActivo { get; set; }
		public DateTime FechaCreacion { get; set; }

		public string FechaFormateada => FechaCreacion.ToString("dd/MM/yyyy HH:mm");
		public string EstadoLabel => EsActivo ? "Activo" : "Inactivo";
	}
}