using System;
using System.Collections.Generic;

namespace ModernTodo.Data;

public partial class AapyTask
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public int ToDoListId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int Priority { get; set; }

    public DateTime? CompletedOn { get; set; }

    public DateTime? Deadline { get; set; }

    public virtual AapyToDoList ToDoList { get; set; } = null!;

    public virtual ICollection<AapyTag> Tags { get; set; } = new List<AapyTag>();
}
