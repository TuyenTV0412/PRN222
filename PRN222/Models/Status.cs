using System;
using System.Collections.Generic;

namespace PRN222.Models;

public partial class Status
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<BorrowDetail> BorrowDetails { get; set; } = new List<BorrowDetail>();
}
