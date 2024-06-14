using ModernTodo.Data;
using Microsoft.EntityFrameworkCore;
using aapy.Data;

namespace ModernTodo.Services
{
	public class AuthService
	{
		private readonly CryptoService256 _cryptoService;
		private readonly AapyDbTodoContext _ctx;
		public AuthService(CryptoService256 cryptoService, AapyDbTodoContext ctx)
		{
			_cryptoService = cryptoService;
			_ctx = ctx;
		}

		public async Task<bool> RegisterNewUserAsync(string mail, string username, string password, string password_conf)
		{
			//Überprüfungen
			if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return false;

			//überprüft nochmal, ob die länge passt
			if (username.Length < 3 || username.Length > 50) return false;

			if (!await IsPasswordEqual(password, password_conf)) return false;

			var existingUser = await _ctx.AapyUsers.FirstOrDefaultAsync(u => u.Mail == mail);
			if (existingUser != null) return false;

			//schaut einfach nach, ob benutzer in der liste sind, sind welche vorhanden, gib er true back
			var isFirstUser = await _ctx.AapyUsers.AnyAsync();

			//Salt erzeugen
			var salt = _cryptoService.GenerateSalt();

			//Salt an Passwort hängen
			var saltedPassword = _cryptoService.SaltString(password, salt, System.Text.Encoding.UTF8);

			//Gesaltetes Passwort Hashen
			var hash = _cryptoService.GetHash(saltedPassword);

			//Benutzer in Datenbank speichern
			var newUser = new AapyUser
			{
				PasswordHash = hash,
				RegisteredOn = DateTime.Now,
				Salt = salt,
				Username = username,
				Mail = mail,
				RoleId = 1,
			};

			if (!isFirstUser)
			{
				newUser.RoleId = 3;
			}
			_ctx.AapyUsers.Add(newUser);

			await _ctx.SaveChangesAsync();
			return true;
		}

		private async Task<bool> IsPasswordEqual(string password, string password_conf)
		{
			if (password == password_conf) return true;

			return false;
		}

		public async Task<bool> CanUserLogInAsync(string mail, string loginPassword)
		{
			//Benutzer in DB suchen und laden
			var dbAppUser = await _ctx.AapyUsers.Where(x => x.Mail == mail).FirstOrDefaultAsync();

			//Wenn Benutzer existiert...
			if (dbAppUser is null) return false;

			//Login-Passwort mit Salt aus DB salten
			var saltedLoginPassword = _cryptoService.SaltString(loginPassword, dbAppUser.Salt, System.Text.Encoding.UTF8);

			//Das gesaltete Login-PW hashen
			var hashedLoginPassword = _cryptoService.GetHash(saltedLoginPassword);

			//Den Login-PW-Hash mit dem Hash des PW aus der DB vergleichen
			//Wenn gleich, dann darf der Benutzer einloggen
			//Sonst nicht
			return hashedLoginPassword.SequenceEqual(dbAppUser.PasswordHash);

			//Achtung! Die beiden byte[]s können NICHT einfach mit == verglichen werden, da es sich um Verweis-Datentypen handelt
			//Bei Verweisdatentypen wird mit == immer die Verweise verglichen (und nicht der Inhalt) 
		}
		internal async Task<string> GetRoleByUsernameAsync(string mail)
		{
			return await _ctx.AapyUsers
				.Where(au => au.Mail == mail)
				.Select(au => au.Role.RoleName)
				.FirstAsync();
		}

		public async Task UpgradeRole(string mail)
		{
			var user = _ctx.AapyUsers.Where(u => u.Mail == mail).FirstOrDefault();

			user.RoleId = 2;

			_ctx.AapyUsers.Update(user);
			_ctx.SaveChanges();

		}

	}
}
