using System.ComponentModel.DataAnnotations;

namespace ModernTodo.Models
{
	public class TodoModel
	{
		public int TodoId { get; set; }
		[Required]
		[MaxLength(30)]
		public string Name { get; set; }
		public int UserId { get; set; }
        public int CompletedTasksCount { get; set; } 
        public int NotCompletedTasksCount { get; set; }
    }

	public class TodoModelVm
	{
		public List<TodoModel> ToDoList { get; set; } = new();
	}
}
