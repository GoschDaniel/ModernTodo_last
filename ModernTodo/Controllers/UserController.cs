using System;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ModernTodo.Data;
using ModernTodo.Models;
using ModernTodo.Services;

namespace ModernTodo.Controllers
{
	[Authorize(Roles = "Free-Tier,Premium-Tier")]
	public class UserController : Controller
	{
		public static string Name = nameof(UserController).Replace("Controller", string.Empty);
		private readonly UserService _todoService;
		private readonly AapyDbTodoContext _ctx;
		private readonly TagService _tagService;
		private readonly AuthService _authService;


		public UserController(UserService todoService, AapyDbTodoContext ctx, TagService tagService, AuthService authService)
		{
			_ctx = ctx;
			_todoService = todoService;
			_tagService = tagService;
			_authService = authService;
		}

		#region Todos
		[HttpGet]
		public async Task<IActionResult> Todos()
		{
			var mail = await GetUserIdentityName();

			var user = _ctx.AapyUsers
			.Include(u => u.Role)
			.FirstOrDefault(u => u.Mail == mail);

			var roleNameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

			if (roleNameClaim != user.Role.RoleName)
			{
				return RedirectToAction(nameof(AuthController.Logout), AuthController.Name);
			}

			var todos = await _todoService.GetAllTodos(mail);

			var vm = new TodoModelVm
			{
				ToDoList = todos.Select(t => new TodoModel
				{
					TodoId = t.Id,
					Name = t.Name,
					UserId = t.UserId,
					CompletedTasksCount = t.AapyTasks.Count(a => a.CompletedOn == null),
					NotCompletedTasksCount = t.AapyTasks.Count(a => a.CompletedOn != null)
				}).ToList()
			};

			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> AddTodo(string name)
		{
			var userIdentityName = await GetUserIdentityName();
			var todos = await _todoService.GetAllTodos(userIdentityName);

			bool isValidName = !string.IsNullOrEmpty(name) && name.Length < 30;
			bool canAddTodo = User.IsInRole("Premium-Tier") && todos.Count < 100 ||
							  User.IsInRole("Free-Tier") && todos.Count < 5;
			var newTodoId = 0;

			if (canAddTodo && isValidName)
			{
				newTodoId = await _todoService.AddTodo(name, userIdentityName);
				TempData["NewTodoId"] = newTodoId;
			}
			else
			{
				TempData["ErrorMessage"] = "Fehler. Bitte Eingabe noch einmal überprüfen!";
				return RedirectToAction(nameof(Todos));
			}

			return RedirectToAction(nameof(Tasks), new { id = newTodoId });
			//return RedirectToAction(nameof(Todos), UserController.Name);
		}



		[HttpGet]
		public async Task<IActionResult> DeleteTodo(int id)
		{
			var todo = await _ctx.AapyToDoLists.FindAsync(id);

			var vm = new TodoModelVm
			{
				ToDoList = new List<TodoModel>
		 {
			new TodoModel
			{
				TodoId = todo.Id,
				Name = todo.Name,
				UserId = todo.UserId,
				CompletedTasksCount = todo.AapyTasks.Count(a => a.CompletedOn != null),
				NotCompletedTasksCount = todo.AapyTasks.Count(a => a.CompletedOn == null)
			}
		}
			};

			return View(vm);
		}


		[HttpPost]
		public async Task<IActionResult> ConfirmDeleteTodo(int id)
		{
			var todo = await _ctx.AapyToDoLists.FindAsync(id);

			_ctx.AapyToDoLists.Remove(todo);
			await _ctx.SaveChangesAsync();

			return RedirectToAction(nameof(Todos), UserController.Name);
		}

		[HttpGet]
		public async Task<IActionResult> EditTodo(int id)
		{
			var todo = await _ctx.AapyToDoLists.FindAsync(id);

			var vm = new TodoModelVm
			{
				ToDoList = new List<TodoModel>
				{
			new TodoModel
					{
						TodoId = todo.Id,
						Name = todo.Name,
						UserId = todo.UserId,
						CompletedTasksCount = todo.AapyTasks.Count(a => a.CompletedOn != null),
						NotCompletedTasksCount = todo.AapyTasks.Count(a => a.CompletedOn == null)
					}
				}
			};

			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> EditTodo(int id, string name)
		{
			var todo = await _ctx.AapyToDoLists.FindAsync(id);

			todo.Name = name;

			_ctx.Update(todo);
			await _ctx.SaveChangesAsync();

			return RedirectToAction(nameof(Todos), UserController.Name);
		}
		#endregion


		#region Tasks
		[HttpGet]
		public async Task<IActionResult> Tasks(int id, List<int> tagIds)
		{
			var tasks = await _todoService.GetAllTasksWithTags(id, tagIds);
			var user = _ctx.AapyUsers.FirstOrDefault(u => u.Mail == User.Identity.Name);
			var tags = await _ctx.AapyTags
								.Where(tag => tag.UserId == null || tag.UserId == user.Id || tagIds.Contains(tag.Id))
								.ToListAsync();

			TempData["NewTodoId"] = id;
			var vm = new TaskModelVm
			{
				TaskList = tasks.Select(t => new TaskModel
				{
					TaskId = t.Id,
					Description = t.Description,
					TodoId = id,
					CreatedOn = t.CreatedOn,
					CompletedOn = t.CompletedOn,
					Deadline = t.Deadline,
					Priority = t.Priority,
					Tags = t.Tags.Select(tag => new TagViewModel
					{
						Id = tag.Id,
						Name = tag.Name,
						HexColor = tag.HexColor
					}).ToList(),
					Urgent = !t.CompletedOn.HasValue && (t.Deadline.HasValue && t.Deadline.Value.Date >= DateTime.Today && t.Deadline.Value.Date <= DateTime.Today.AddDays(7))
				}).ToList(),
				AvailableTags = tags.Select(tag => new TagViewModel
				{
					Id = tag.Id,
					Name = tag.Name,
					HexColor = tag.HexColor
				}).ToList()
			};
			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> AddTask(TaskModel model)
		{
			var userIdentityName = await GetUserIdentityName();
			var tasks = await _todoService.GetAllTasksWithTags(model.TodoId, model.TagIds); ;

			bool isValidName = !string.IsNullOrEmpty(model.Description) && model.Description.Length < 30;
			bool canAddTask = User.IsInRole("Premium-Tier") && tasks.Count < 1000 ||
							  User.IsInRole("Free-Tier") && tasks.Count < 20;
			var todoId = 0;

			if (canAddTask && isValidName)
			{
				todoId = await _todoService.AddTask(model);
				return RedirectToAction(nameof(Tasks), new { id = model.TodoId });
			}
			else
			{
				TempData["ErrorMessage"] = "Fehler. Bitte Eingabe noch einmal überprüfen!";

			}
			return RedirectToAction(nameof(Tasks), new { id = model.TodoId });

		}


		[HttpGet]
		public async Task<IActionResult> DeleteTask(int id)
		{
			var task = await _ctx.AapyTasks
								 .Include(t => t.Tags)
								 .FirstOrDefaultAsync(t => t.Id == id);

			var vm = new TaskModelVm
			{
				TaskList = new List<TaskModel>
		{
			new TaskModel
			{
				TaskId = task.Id,
				TodoId = task.ToDoListId,
				Description = task.Description,
				CreatedOn = task.CreatedOn,
				CompletedOn = task.CompletedOn,
				Deadline = task.Deadline,
				Priority = task.Priority,
				Tags = task.Tags.Select(tag => new TagViewModel
				{
					Id = tag.Id,
					Name = tag.Name,
					HexColor = tag.HexColor
				}).ToList()
			}
		}
			};

			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> ConfirmDeleteTask(int id)
		{
			//problem mit dem löschen, cascade?? wegen tasktags
			var task = await _ctx.AapyTasks.FindAsync(id);

			_ctx.AapyTasks.Remove(task);
			await _ctx.SaveChangesAsync();

			return RedirectToAction(nameof(Tasks), new { id = task?.ToDoListId });
		}

		[HttpGet]
		public async Task<IActionResult> EditTask(int id, List<int> tagIds)
		{
			var task = await _ctx.AapyTasks
								 .Include(t => t.Tags)
								 .FirstOrDefaultAsync(t => t.Id == id);

			var tags = await _ctx.AapyTags.Where(tag => tag.UserId == null || tagIds.Contains(tag.Id)).ToListAsync();

			var vm = new TaskModelVm
			{
				TaskList = new List<TaskModel>
				{
					new TaskModel
					{
						TaskId = task.Id,
						TodoId = task.ToDoListId,
						Description = task.Description,
						CreatedOn = task.CreatedOn,
						CompletedOn = task.CompletedOn,
						Deadline = task.Deadline,
						Priority = task.Priority,
						Tags = task.Tags.Select(tag => new TagViewModel
						{
							Id = tag.Id,
							Name = tag.Name,
							HexColor = tag.HexColor
						}).ToList()

					}

				},
				AvailableTags = tags.Select(tag => new TagViewModel
				{
					Id = tag.Id,
					Name = tag.Name,
					HexColor = tag.HexColor
				}).ToList()

			};

			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> EditTask(int taskId, string description, DateTime completedOn, DateTime deadline, int priority, int todoId, List<int> tagIds)
		{
			var task = await _ctx.AapyTasks
								 .Include(t => t.Tags)
								 .FirstOrDefaultAsync(t => t.Id == taskId);


			if (!string.IsNullOrEmpty(description))
			{
				task.Description = description;
			}
			if (completedOn != DateTime.MinValue)
			{
				task.CompletedOn = completedOn;
			}
			if (deadline != DateTime.MinValue)
			{
				task.Deadline = deadline;
			}
			if (priority >= 0 && priority <= 3)
			{
				task.Priority = priority;
			}
			if (tagIds != null && tagIds.Any())
			{
				var selectedTags = await _ctx.AapyTags.Where(tag => tagIds.Contains(tag.Id)).ToListAsync();
				task.Tags = selectedTags;
			}


			await _ctx.SaveChangesAsync();

			return RedirectToAction(nameof(Tasks), new { id = todoId });
		}

		public async Task<IActionResult> DeleteAllCompletetedTasks(int todoId)
		{
			var completedTasks = await _ctx.AapyTasks.Where(t => t.ToDoListId == todoId && t.CompletedOn != null).ToListAsync();
			foreach (var task in completedTasks)
			{
				_ctx.AapyTasks.Remove(task);
			}
			await _ctx.SaveChangesAsync();

			return RedirectToAction(nameof(Tasks), new { id = todoId });
		}

		[HttpGet]
		public async Task<IActionResult> CompleteTask(int id)
		{
			var task = _ctx.AapyTasks.Where(t => t.Id == id).FirstOrDefault();

			task.CompletedOn = DateTime.Now;
			await _ctx.SaveChangesAsync();

			return RedirectToAction(nameof(Tasks), new { id = task.ToDoListId });
		}
		#endregion

		#region Tags
		public async Task<IActionResult> AddTag(string name, string hexColor, string todoId)
		{
			var userIdentityName = await GetUserIdentityName();
			var tags = await _tagService.GetAllTags(userIdentityName);

			bool isValidName = !string.IsNullOrEmpty(name) && name.Length < 10;
			bool canAddTodo = (User.IsInRole("Premium-Tier") || User.IsInRole("Free-Tier")) && tags.Count < 20;

			if (canAddTodo && isValidName)
			{
				hexColor = hexColor.Trim(new Char[] { '#' });
				await _tagService.AddTag(name, hexColor, await GetUserIdentityName());
			}
			else
			{
				TempData["ErrorMessage"] = "Fehler. Bitte Eingabe noch einmal überprüfen!";

			}
			return RedirectToAction(nameof(Tags));
		}

		[HttpGet]
		public async Task<IActionResult> Tags()
		{
			var tag = await _tagService.GetAllTags(await GetUserIdentityName());

			var vm = new TaskModelVm
			{
				AvailableTags = tag.Select(t => new TagViewModel
				{
					Id = t.Id,
					Name = t.Name,
					HexColor = t.HexColor
				}).ToList()
			};

			return View(vm);
		}

		[HttpGet]
		public async Task<IActionResult> EditTag(int id)
		{
			var tag = await _ctx.AapyTags.FindAsync(id);

			var vm = new TaskModelVm
			{
				AvailableTags = new List<TagViewModel>
				{
					new TagViewModel
					{
					Id = tag.Id,
					Name = tag.Name,
					HexColor = tag.HexColor
					}
				}
			};

			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> SaveEditTag(TagViewModel model)
		{
			var tag = await _ctx.AapyTags.FindAsync(model.Id);

			tag.Name = model.Name;
			tag.HexColor = model.HexColor.Trim(new Char[] { '#' });

			_ctx.Update(tag);
			await _ctx.SaveChangesAsync();

			return RedirectToAction(nameof(Tags));
		}

		[HttpGet]
		public async Task<IActionResult> DeleteTag(int id)
		{
			var tag = await _ctx.AapyTags.FindAsync(id);

			bool isTagAssignedToTask = await _ctx.AapyTasks.AnyAsync(t => t.Tags.Any(tt => tt.Id == id));

			if (isTagAssignedToTask)
			{
				TempData["ErrorMessage"] = "Der Tag kann nicht gelöscht werden, da er einer oder mehreren Tasks zugewiesen ist.";
				return RedirectToAction(nameof(Tags), UserController.Name);
			}

			var vm = new TaskModelVm
			{
				AvailableTags = new List<TagViewModel>
				{
					new TagViewModel
					{
						Id = tag.Id,
						Name = tag.Name,
						HexColor = tag.HexColor
					}
				}
			};

			return View(vm);
		}



		[HttpPost]
		public async Task<IActionResult> ConfirmDeleteTag(int id)
		{
			var tag = await _ctx.AapyTags.FindAsync(id);

			_ctx.AapyTags.Remove(tag);
			await _ctx.SaveChangesAsync();

			return RedirectToAction(nameof(Tags), UserController.Name);
		}
		#endregion

		[HttpPost]
		public async Task<IActionResult> UpgradeRole()
		{

			await _authService.UpgradeRole(await GetUserIdentityName());

			return RedirectToAction(nameof(AuthController.Logout), AuthController.Name);
		}

		public async Task<string> GetUserIdentityName()
		{
			return User.Identity.Name;
		}


	}
}
