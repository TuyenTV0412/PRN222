using System;
using System.Collections.Generic;

namespace PRN222.Models;

public partial class Punish
{
    public int PunishId { get; set; }

    public int BookId { get; set; }

    public int PersonId { get; set; }

    public string PunishDetail { get; set; } = null!;

    public virtual Book Book { get; set; } = null!;

    public virtual User Person { get; set; } = null!;
}
