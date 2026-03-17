using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gestion_Usuarios.Models
{
	public class StudentEditViewModel
	{
		public int Id { get; set; }

		[StringLength(20, ErrorMessage = "La matrícula no puede exceder 20 caracteres.")]
		[Display(Name = "Matrícula")]
		public string? Matricula { get; set; }

		[StringLength(20, ErrorMessage = "El folio no puede exceder 20 caracteres.")]
		[Display(Name = "Folio")]
		public string? Folio { get; set; }

		[Required(ErrorMessage = "Los nombres son obligatorios.")]
		[StringLength(100, ErrorMessage = "Los nombres no pueden exceder 100 caracteres.")]
		[Display(Name = "Nombre(s)")]
		public string Nombres { get; set; } = string.Empty;

		[Required(ErrorMessage = "El apellido paterno es obligatorio.")]
		[StringLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres.")]
		[Display(Name = "Apellido paterno")]
		public string ApellidoPaterno { get; set; } = string.Empty;

		[StringLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres.")]
		[Display(Name = "Apellido materno")]
		public string? ApellidoMaterno { get; set; }

		[RegularExpression(@"^$|^[A-Za-z0-9]{18}$", ErrorMessage = "La CURP debe tener 18 caracteres alfanuméricos.")]
		public string? CURP { get; set; }

		[EmailAddress(ErrorMessage = "Captura un correo electrónico válido.")]
		[Display(Name = "Correo electrónico")]
		public string? Email { get; set; }

		[Display(Name = "Carrera")]
		public int? CareerId { get; set; }

		[Display(Name = "Grupo")]
		public int? GroupId { get; set; }

		public int? Semestre { get; set; }

		public string EstadoCodigo { get; set; } = string.Empty;

		public bool EsActivo { get; set; }

		public string? ReturnUrl { get; set; }

		public List<LookupOptionViewModel> CareerOptions { get; set; } = new();

		public List<LookupOptionViewModel> GroupOptions { get; set; } = new();

		public string Identificador =>
			!string.IsNullOrWhiteSpace(Matricula)
				? Matricula
				: Folio ?? string.Empty;
	}
}
