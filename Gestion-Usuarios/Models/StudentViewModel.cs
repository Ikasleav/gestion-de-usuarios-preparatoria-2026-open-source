using System;
using System.ComponentModel.DataAnnotations;

namespace Gestion_Usuarios.Models
{
	public class StudentViewModel
	{
		public int Id { get; set; }

		public string? Matricula { get; set; }

		public string? Folio { get; set; }

		// Preferencia: Matrícula, si no existe Folio, si no empty
		public string Identificador => Matricula ?? Folio ?? string.Empty;

		// Datos Personales
		public string Nombres { get; set; } = string.Empty;
		public string ApellidoPaterno { get; set; } = string.Empty;
		public string? ApellidoMaterno { get; set; }

		// Nombre completo calculado y seguro ante nulos/espacios
		public string NombreCompleto
		{
			get
			{
				var parts = new[] { Nombres, ApellidoPaterno, ApellidoMaterno };
				return string.Join(' ', parts).Replace("  ", " ").Trim();
			}
		}

		public string Carrera { get; set; } = "Sin Asignar";

		public int? Semestre { get; set; }

		public string EstadoCodigo { get; set; } = string.Empty;

		public bool EsActivo { get; set; }

		public string? CURP { get; set; }

		public string? Email { get; set; }

		public string Grupo { get; set; } = string.Empty;

		// Clase CSS para badge, centralizada aquí
		public string EstadoBadgeClass => !EsActivo
			? "bg-danger"
			: EstadoCodigo?.ToUpper() switch
			{
				"INSCRITO" => "bg-success",
				"PREINSCRITO" => "bg-warning text-dark",
				"BAJA" => "bg-danger",
				_ => "bg-secondary"
			};
	}
}