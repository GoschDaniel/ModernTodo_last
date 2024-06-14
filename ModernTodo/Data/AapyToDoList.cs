using System;
using System.Collections.Generic;

namespace ModernTodo.Data;

public partial class AapyToDoList
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int UserId { get; set; }

    public virtual ICollection<AapyTask> AapyTasks { get; set; } = new List<AapyTask>();

    public virtual AapyUser User { get; set; } = null!;
}
