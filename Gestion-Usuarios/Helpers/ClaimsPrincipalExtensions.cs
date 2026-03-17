using System;
using System.Linq;
using System.Security.Claims;

namespace Gestion_Usuarios.Helpers
{
	public static class ClaimsPrincipalExtensions
	{
		public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
		{
			if (principal?.Identity?.IsAuthenticated != true || roles == null || roles.Length == 0)
			{
				return false;
			}

			var claimRoles = principal.Claims
				.Where(c => c.Type == ClaimTypes.Role)
				.Select(c => c.Value)
				.ToList();

			return roles.Any(role => claimRoles.Any(claimRole =>
				string.Equals(claimRole, role, StringComparison.OrdinalIgnoreCase)));
		}
	}
}
