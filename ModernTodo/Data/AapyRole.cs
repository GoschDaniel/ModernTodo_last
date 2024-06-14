using System;
using System.Collections.Generic;

namespace ModernTodo.Data;

public partial class AapyRole
{
    public int Id { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<AapyUser> AapyUsers { get; set; } = new List<AapyUser>();
}
