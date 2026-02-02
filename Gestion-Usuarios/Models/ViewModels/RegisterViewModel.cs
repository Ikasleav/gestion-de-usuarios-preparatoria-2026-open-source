using System.ComponentModel.DataAnnotations;

namespace Gestion_Usuarios.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(80)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}