using System.ComponentModel.DataAnnotations;
using Gestion_Usuarios.Models;

namespace Gestion_Usuarios.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ApellidoMaterno { get; set; }

        [Required]
        [StringLength(80)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
