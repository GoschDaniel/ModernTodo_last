using aapy.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernTodo.Data;
using ModernTodo.Models;
using ModernTodo.Services;

namespace ModernTodo.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		public static string Name = nameof(AdminController).Replace("Controller", string.Empty);
		private readonly AdminService _adminService;
		private readonly AapyDbTodoContext _ctx;
		private readonly AuthService _authService;
		private readonly CryptoService256 _cryptoService;

		public AdminController(AdminService adminService, AapyDbTodoContext ctx, AuthService authService, CryptoService256 cryptoService)
		{
			_adminService = adminService;
			_ctx = ctx;
			_authService = authService;
			_cryptoService = cryptoService;
		}

		[HttpGet]
		public async Task<IActionResult> Adminpanel()
		{
			var users = await _adminService.GetAllUsersWithStats();
			var availableRoles = await _adminService.GetAvailableRoles();

			var vm = new UserModelVm
			{
				UserModelList = users.Select(u => new UserModel
				{
					UserId = u.Id,
					Mail = u.Mail,
					Username = u.Username,
					RegisteredOn = u.RegisteredOn,
					Role = new UserViewRole
					{
						RoleId = u.RoleId,
						RoleName = u.Role?.RoleName,
					},
					NumberOfLists = u.AapyToDoLists.Count,
					NumberOfTasks = u.AapyToDoLists.Sum(tl => tl.AapyTasks.Count),
					NumberOfTags = _ctx.AapyTags.Where(t => t.UserId == u.Id).Count()
				}).ToList(),
				AvailableRoles = availableRoles
			};

			return View(vm);
		}


		[HttpPost]
		public async Task<IActionResult> UpdateUserRole(UserModel model)
		{
			var user = await _adminService.GetUserById(model.UserId);

			user.RoleId = model.Role.RoleId;
			await _adminService.UpdateUser(user);

			return RedirectToAction(nameof(Adminpanel));
		}

		[HttpPost]
		public async Task<IActionResult> ResetPassword(int userId)
		{
            var user = await _adminService.GetUserById(userId);
			TempData["Message"] = $"Passwort von {user.Mail} wurde zu 'admin' geändert.";

			var salt = _cryptoService.GenerateSalt();

			//Salt an Passwort hängen
			var saltedPassword = _cryptoService.SaltString("admin", salt, System.Text.Encoding.UTF8);

			//Gesaltetes Passwort Hashen
			var hash = _cryptoService.GetHash(saltedPassword);

			user.Salt = salt;
			user.PasswordHash = hash;

			await _ctx.SaveChangesAsync();
			return RedirectToAction(nameof(Adminpanel));
		}



		public async Task<string> GetUserIdentityName()
		{
			return User.Identity.Name;
		}

	}
}
