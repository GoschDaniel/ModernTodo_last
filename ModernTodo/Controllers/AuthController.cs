using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ModernTodo.Services;

namespace ModernTodo.Controllers
{
    public class AuthController : Controller
    {
        public static string Name = nameof(AuthController).Replace("Controller", null);

        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
		[HttpGet]
		public async Task<IActionResult> Register()
        {
           return View();
        }

		[HttpPost]
		public async Task<IActionResult> Register(string mail, string username, string password, string password_conf)
		{
			//damit die eingabe felder nach absenden nicht leer sind, wenn die eingabe falsch war. 
			ViewData["Mail"] = mail;
			ViewData["Username"] = username;

			if (!await _authService.RegisterNewUserAsync(mail, username, password, password_conf))
			{
				TempData["ErrorMessage"] = "Fehler. Bitte Eingabe noch einmal überprüfen!";
				return View("Register");

			}

			Console.WriteLine($"Jemand hat sich mit {mail}, {username} und {password} registriert");
			return RedirectToAction(nameof(Login));

		}

		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}

        [HttpPost]
        public async Task<IActionResult> Login(string mail, string password)
        {
            var userCanLogIn = await _authService.CanUserLogInAsync(mail, password);
            Console.WriteLine($"Jemand hat sich mit {mail} und {password} eingeloggt: {userCanLogIn}");

            if (userCanLogIn)
            {
                // Benutzer authentifizieren und in die Webanwendung einloggen

                await LogUserIntoWebApp(mail);

                // Überprüfe die Rolle des Benutzers
                var userRolename = await _authService.GetRoleByUsernameAsync(mail);

				if (userRolename == "Admin")
				{
					return RedirectToAction(nameof(AdminController.Adminpanel), AdminController.Name);
				}
				else
				{
					return RedirectToAction(nameof(UserController.Todos), UserController.Name);
				}

			}
			else
            {
                // Wenn die Anmeldeinformationen ungültig sind, zurück zur Login-Seite mit Fehlermeldung
                TempData["ErrorMessage"] = "Fehler. Bitte Eingabe noch einmal überprüfen!";
				TempData["Mail"] = mail;
                return RedirectToAction(nameof(Login));
            }
            return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
        }

        [HttpGet]
		public IActionResult Logout()
		{
			HttpContext.SignOutAsync();

			return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
		}

		[NonAction]
		private async Task LogUserIntoWebApp(string mail)
		{
			//1. Claims (Behauptungen) über den Benutzer zusammentragen
			//Ein Claim ist einfach nur ein Key-Value-Pair - Ein Wert mit einem bestimmten Namen
			var claim = new Claim("LastLogin", DateTime.Now.ToString());

			//Damit das ASP.NET Core Auth-System mit Cookies funktioniert, ist ein Claim besonders wichtig: Der Name-Claim
			var nameClaim = new Claim(ClaimTypes.Name, mail);

			//hier wird die funktion aufgerufen, die anhand der userid die roleids/name holt
			string userRolename = await _authService.GetRoleByUsernameAsync(mail);

			//Soll ein benutzer eine oder mehrere Rollen haben, müssen Role-Claims hinzugefügt werden
			var roleClaim = new Claim(ClaimTypes.Role, userRolename);

			//Alle Claims die dieser Liste (und damit der Identity bzw. dem Principal) hinzugefügt werden,
			//...werden dann im Auth-Cookie gespeichert (und können dort auch wieder ausgelesen werden)
			var claims = new List<Claim>()
			{
				claim, nameClaim, roleClaim
			};


			//2. Mit den Claims eine Identität erstellen
			//In unserem Fall benötigt der Principal eine Identität, da wir einzelne Benutzer voneinander unterscheiden wollen
			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

			//3. Die Identität einem Rechteinhaber zuweisen
			//In der Microsoft bzw. .NET Welt ist ein "Principal" ein Rechteinhaber
			var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);


			//4. Den Rechteinhaber in der Anwendung "registrieren"
			//Um einen Benutzer in der Webanwendung als "eingeloggt" zu "markieren" (und ihm das Auth-Cookie zu schicken),
			//gibt es die Methode SignInAsync

			HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				claimsPrincipal
				);

			//Der ClaimsPrincipal der hier erzeugt und "registriert" wird, ist nachher auch über eine spezielle Variable
			//...in Controllern und Views abrufbar. Die Variable heißt "User" und enthält neben allen Informationen
			//...die über die Claims hier gespeichert werden, auch nützliche Hilfsmethoden/-Eigenschaften.

			//zB
			//User.Identity.Name erlaubt Zugriff auf den abgespeicherten Namen
			//User.Identity.IsAuthenticated meldet, ob gerade ein Benutzer eingeloggt ist oder nicht (nützlich, um zB
			//...Interface-Elemente anzuzeigen bzw. zu verstecken, je nachdem ob ein eingeloggter oder anonymer Benutzer
			//...die View gerade ansieht).
			//User.Claims erlaubt Zugriff auf alle gespeicherten Claims
		}
	}
}
