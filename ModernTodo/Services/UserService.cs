using Microsoft.EntityFrameworkCore;
using ModernTodo.Data;
using Microsoft.AspNetCore.Authorization;
using ModernTodo.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Runtime.CompilerServices;

namespace ModernTodo.Services;

public class UserService
{
	private readonly AapyDbTodoContext _ctx;

	public UserService(AapyDbTodoContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<List<AapyToDoList>> GetAllTodos(string mail)
	{
		var user = await _ctx.AapyUsers.FirstOrDefaultAsync(u => u.Mail == mail);

		var todos = await _ctx.AapyToDoLists
			.Include(t => t.User)
			.Include(t => t.AapyTasks)
			.Where(t => t.UserId == user.Id)
			.ToListAsync();

		return todos;
	}

	public async Task<int> AddTodo(string name, string mail)
	{
		var user = await _ctx.AapyUsers.Where(t => t.Mail == mail).FirstOrDefaultAsync();

		var newTodo = new AapyToDoList { Name = name, UserId = user.Id };

		_ctx.AapyToDoLists.Add(newTodo);
		await _ctx.SaveChangesAsync();
		return newTodo.Id;
	}

	public async Task DeleteTodoFinal(int id)
	{
		var todo = await _ctx.AapyToDoLists.Where(t => t.Id == id).FirstOrDefaultAsync();

		_ctx.AapyToDoLists.Remove(todo);
		await _ctx.SaveChangesAsync();
	}

	public async Task<List<AapyTask>> GetAllTasksWithTags(int toDoListId, List<int> tagIds)
	{
		var tasks = await _ctx.AapyTasks
			.Include(t => t.Tags)
			.Where(t => t.ToDoList.Id == toDoListId)
			.ToListAsync();

		return tasks;
	}


	public async Task<int> AddTask(TaskModel model)
	{
		// Überprüfen, ob die ToDoListId existiert
		var toDoListExists = await _ctx.AapyToDoLists.AnyAsync(list => list.Id == model.TodoId);

		var newTask = new AapyTask
		{
			Description = model.Description,
			CreatedOn = DateTime.Now,
			CompletedOn = model.CompletedOn,
			Deadline = model.Deadline,
			Priority = model.Priority,
			ToDoListId = model.TodoId
		};

		if (model.TagIds != null && model.TagIds.Count > 0)
		{
			var tags = await _ctx.AapyTags.Where(tag => model.TagIds.Contains(tag.Id)).ToListAsync();
			newTask.Tags = tags;
		}

		_ctx.AapyTasks.Add(newTask);
		await _ctx.SaveChangesAsync();
		return newTask.ToDoListId;
	}










}
