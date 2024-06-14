using System;
using System.Collections.Generic;

namespace ModernTodo.Data;

public partial class AapyTag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string HexColor { get; set; } = null!;

    public int? UserId { get; set; }

    public virtual ICollection<AapyTask> Tasks { get; set; } = new List<AapyTask>();
}
