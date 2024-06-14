using Microsoft.EntityFrameworkCore;
using ModernTodo.Data;
using ModernTodo.Models;

namespace ModernTodo.Services
{
	public class AdminService
	{
		private readonly AapyDbTodoContext _ctx;

		public AdminService(AapyDbTodoContext ctx)
		{
			_ctx = ctx;
		}
		public async Task<List<AapyUser>> GetAllUsersWithStats()
		{
			var users = await _ctx.AapyUsers
							  .Include(u => u.Role)
							  .OrderBy(u => u.Mail)
							  .Include(u => u.AapyToDoLists)
								  .ThenInclude(tl => tl.AapyTasks)
							  .ToListAsync();

			return users;
		}

		public async Task<AapyUser> GetUserById(int id)
		{
			return await _ctx.AapyUsers.FirstOrDefaultAsync(u => u.Id == id);
		}

		public async Task UpdateUser(AapyUser user)
		{
			_ctx.Entry(user).State = EntityState.Modified;
			await _ctx.SaveChangesAsync();
		}

		public async Task<List<UserViewRole>> GetAvailableRoles()
		{
			return await _ctx.AapyRoles
				.Select(r => new UserViewRole
				{
					RoleId = r.Id,
					RoleName = r.RoleName
				})
				.ToListAsync();
		}


	}
}
