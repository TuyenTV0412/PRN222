using System;
using System.Collections.Generic;

namespace PRN222.Models;

public partial class Borrow
{
    public int BorrowId { get; set; }

    public int PersonId { get; set; }

    public DateOnly BorrowDate { get; set; }

    public DateOnly Deadline { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public virtual ICollection<BorrowDetail> BorrowDetails { get; set; } = new List<BorrowDetail>();

    public virtual User Person { get; set; } = null!;
}
