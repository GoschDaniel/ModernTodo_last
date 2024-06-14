using System;
using System.Collections.Generic;

namespace ModernTodo.Data;

public partial class AapyUser
{
    public int Id { get; set; }

    public string Mail { get; set; } = null!;

    public string Username { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public byte[] Salt { get; set; } = null!;

    public DateTime RegisteredOn { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<AapyToDoList> AapyToDoLists { get; set; } = new List<AapyToDoList>();

    public virtual AapyRole Role { get; set; } = null!;
}
