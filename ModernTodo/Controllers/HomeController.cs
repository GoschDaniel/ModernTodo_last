using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ModernTodo.Models;
using ModernTodo.Services;

namespace ModernTodo.Controllers
{
	public class HomeController : Controller
	{
		public static string Name = nameof(HomeController).Replace("Controller", null);
		private readonly ILogger<HomeController> _logger;
		private readonly AuthService _authService;

		public HomeController(ILogger<HomeController> logger, AuthService authService)
		{
			_logger = logger;
			_authService = authService;
		}

		public async Task<IActionResult> Index()
		{
			//if (User.Identity.IsAuthenticated)
			//{
			//	var userRolename = await GetUserIdentityName();
			//	if (userRolename == "Admin")
			//	{
			//		return RedirectToAction(nameof(AdminController.Adminpanel), AdminController.Name);
			//	}
			//	else
			//	{
			//		return RedirectToAction(nameof(UserController.Todos), UserController.Name);
			//	}
			//}
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public async Task<string> GetUserIdentityName()
		{
			return User.Identity.Name;
		}
	}
}
