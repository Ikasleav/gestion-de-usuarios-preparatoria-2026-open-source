using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gestion_Usuarios.Models
{
	public class UsuarioEditViewModel
	{
		public int Id { get; set; }
		public int? PersonId { get; set; }

		[Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
		[StringLength(80, ErrorMessage = "El nombre de usuario no puede exceder 80 caracteres.")]
		[Display(Name = "Usuario")]
		public string Username { get; set; } = string.Empty;

		[EmailAddress(ErrorMessage = "Captura un correo valido.")]
		[Display(Name = "Correo electronico")]
		public string? Email { get; set; }

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

		[StringLength(18, ErrorMessage = "La CURP no puede exceder 18 caracteres.")]
		public string? CURP { get; set; }

		[Phone(ErrorMessage = "Captura un telefono valido.")]
		[Display(Name = "Telefono")]
		public string? Telefono { get; set; }

		[Display(Name = "Rol")]
		[Required(ErrorMessage = "Selecciona un rol.")]
		public int? RoleId { get; set; }

		[Display(Name = "Carrera")]
		[Required(ErrorMessage = "Selecciona una carrera.")]
		public int? CareerId { get; set; }

		[Display(Name = "Contrasena")]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres.")]
		public string? Password { get; set; }

		[Display(Name = "Confirmar contrasena")]
		[Compare(nameof(Password), ErrorMessage = "Las contrasenas no coinciden.")]
		public string? ConfirmPassword { get; set; }

		public bool EsActivo { get; set; } = true;
		public bool IsLocked { get; set; }

		[StringLength(200, ErrorMessage = "La razon no puede exceder 200 caracteres.")]
		[Display(Name = "Razon de bloqueo")]
		public string? LockReason { get; set; }

		public string? ReturnUrl { get; set; }

		public List<LookupOptionViewModel> RoleOptions { get; set; } = new();
		public List<LookupOptionViewModel> CareerOptions { get; set; } = new();
	}
}
