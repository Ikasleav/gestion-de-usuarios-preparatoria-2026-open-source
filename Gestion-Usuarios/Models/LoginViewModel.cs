using System.ComponentModel.DataAnnotations;

namespace Gestion_Usuarios.Models.ViewModels
{
	public class LoginViewModel
	{
		[Required]
		public string UserOrEmail { get; set; }

		[Required]
		public string Password { get; set; }
	}
}