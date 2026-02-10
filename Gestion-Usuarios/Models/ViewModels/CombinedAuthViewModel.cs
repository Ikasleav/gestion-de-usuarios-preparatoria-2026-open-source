using Gestion_Usuarios.Models.ViewModels;

namespace Gestion_Usuarios.Models.ViewModels
{
    public class CombinedAuthViewModel
    {
        public LoginViewModel? Login { get; set; }
        public RegisterViewModel? Register { get; set; }
    }
}   