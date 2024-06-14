using System.ComponentModel.DataAnnotations;

namespace ModernTodo.Models
{
	public class TaskModel
	{
		public int TaskId { get; set; }
		[Required]
		[MaxLength(100)]
		public string Description { get; set; }
		public int TodoId { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime? CompletedOn { get; set; }
		public DateTime? Deadline { get; set; }
		public int Priority { get; set; }
		public List<TagViewModel> Tags { get; set; }
		public List<int> TagIds { get; set; } = new List<int>();
		public bool Urgent { get; set; }

	}

	public class TaskModelVm
	{
		public List<TaskModel> TaskList { get; set; } = new();
		public List<TagViewModel> AvailableTags { get; set; } = new List<TagViewModel>();
	}
	public class TagViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string HexColor { get; set; }
	}
}
