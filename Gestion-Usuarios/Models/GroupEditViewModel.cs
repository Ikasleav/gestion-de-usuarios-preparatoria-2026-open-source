using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gestion_Usuarios.Models
{
	public class GroupEditViewModel
	{
		public int Id { get; set; }

		[Display(Name = "Carrera")]
		public int? CareerId { get; set; }

		[Required(ErrorMessage = "El codigo del grupo es obligatorio.")]
		[StringLength(30, ErrorMessage = "El codigo no puede exceder 30 caracteres.")]
		[Display(Name = "Codigo")]
		public string Codigo { get; set; } = string.Empty;

		[StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
		[Display(Name = "Nombre")]
		public string? Nombre { get; set; }

		[StringLength(20, ErrorMessage = "El turno no puede exceder 20 caracteres.")]
		[Display(Name = "Turno")]
		public string? Turno { get; set; }

		public bool EsActivo { get; set; } = true;

		public string? ReturnUrl { get; set; }

		public List<LookupOptionViewModel> CareerOptions { get; set; } = new();
	}
}
