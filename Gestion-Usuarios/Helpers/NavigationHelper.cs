using System;
using Microsoft.AspNetCore.Mvc;

namespace Gestion_Usuarios.Helpers
{
	public static class NavigationHelper
	{
		public static string ResolveReturnUrl(Controller controller, string? returnUrl, string fallbackUrl)
		{
			if (!string.IsNullOrWhiteSpace(returnUrl) && controller.Url.IsLocalUrl(returnUrl))
			{
				return returnUrl;
			}

			var referer = controller.Request.Headers.Referer.ToString();
			if (Uri.TryCreate(referer, UriKind.Absolute, out var uri))
			{
				var localUrl = uri.PathAndQuery + uri.Fragment;
				if (controller.Url.IsLocalUrl(localUrl))
				{
					return localUrl;
				}
			}

			return fallbackUrl;
		}

		public static IActionResult RedirectToReturnUrl(
			Controller controller,
			string? returnUrl,
			string fallbackAction,
			string? fallbackController = null)
		{
			if (!string.IsNullOrWhiteSpace(returnUrl) && controller.Url.IsLocalUrl(returnUrl))
			{
				return controller.LocalRedirect(returnUrl);
			}

			return string.IsNullOrWhiteSpace(fallbackController)
				? controller.RedirectToAction(fallbackAction)
				: controller.RedirectToAction(fallbackAction, fallbackController);
		}
	}
}
