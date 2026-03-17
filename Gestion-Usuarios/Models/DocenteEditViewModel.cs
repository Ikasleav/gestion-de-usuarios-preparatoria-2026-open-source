using System.ComponentModel.DataAnnotations;

namespace Gestion_Usuarios.Models
{
	public class DocenteEditViewModel
	{
		public int Id { get; set; }
		public int PersonId { get; set; }

		[Required(ErrorMessage = "El nombre es obligatorio.")]
		[StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
		[Display(Name = "Nombre")]
		public string Nombre { get; set; } = string.Empty;

		[Required(ErrorMessage = "El apellido paterno es obligatorio.")]
		[StringLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres.")]
		[Display(Name = "Apellido paterno")]
		public string ApellidoPaterno { get; set; } = string.Empty;

		[StringLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres.")]
		[Display(Name = "Apellido materno")]
		public string? ApellidoMaterno { get; set; }

		[EmailAddress(ErrorMessage = "Captura un correo valido.")]
		[Display(Name = "Correo electronico")]
		public string? Email { get; set; }

		[Phone(ErrorMessage = "Captura un telefono valido.")]
		[Display(Name = "Telefono")]
		public string? Telefono { get; set; }

		[StringLength(30, ErrorMessage = "El numero de empleado no puede exceder 30 caracteres.")]
		[Display(Name = "Numero de empleado")]
		public string? NumeroEmpleado { get; set; }

		[Required(ErrorMessage = "El estatus es obligatorio.")]
		[StringLength(30, ErrorMessage = "El estatus no puede exceder 30 caracteres.")]
		[Display(Name = "Estatus")]
		public string EstadoCodigo { get; set; } = "ACTIVO";

		public bool EsActivo { get; set; } = true;

		public string? ReturnUrl { get; set; }
	}
}
