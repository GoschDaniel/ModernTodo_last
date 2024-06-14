using aapy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ModernTodo.Data;

namespace ModernTodo.Services
{
	public class TagService
	{
		private readonly AapyDbTodoContext _ctx;
		public TagService(AapyDbTodoContext ctx)
		{
			_ctx = ctx;
		}


		public async Task AddTag(string name, string hexColor, string mail)
		{
			var user = await _ctx.AapyUsers.Where(t => t.Mail == mail).FirstOrDefaultAsync();

			var newTag = new AapyTag { Name = name, HexColor = hexColor, UserId = user.Id };

			_ctx.AapyTags.Add(newTag);
			await _ctx.SaveChangesAsync();
		}

		public async Task<List<AapyTag>> GetAllTags(string mail)
		{
			var user = await _ctx.AapyUsers.FirstOrDefaultAsync(u => u.Mail == mail);

			var tags = await _ctx.AapyTags
				.Where(t => t.UserId == user.Id || t.UserId == null)
				.ToListAsync();

			return tags;
		}
	}
}
