namespace ModernTodo.Models
{
	public class UserModel
	{
		public int UserId { get; set; }
		public string Mail { get; set; }
		public string Username { get; set; }
		public DateTime RegisteredOn { get; set; }
		public UserViewRole Role { get; set; }
		public int NumberOfLists { get; set; }  
		public int NumberOfTasks { get; set; } 
		public int NumberOfTags { get; set; }
	}

	public class UserModelVm
	{
		public List<UserModel> UserModelList { get; set; } = new List<UserModel>();
		public List<UserViewRole> AvailableRoles { get; set; } = new List<UserViewRole>();
	}


	public class UserViewRole
	{
		public int RoleId { get; set; }
		public string RoleName { get; set; }
	}


}
