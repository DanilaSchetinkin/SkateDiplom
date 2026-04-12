using System;
using System.Collections.Generic;

namespace SchetinkinDemo.Models;

public partial class UserLogin
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime LoginTime { get; set; }

    public virtual User User { get; set; } = null!;
}
