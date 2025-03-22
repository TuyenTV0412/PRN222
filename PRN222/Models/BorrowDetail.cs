using System;
using System.Collections.Generic;

namespace PRN222.Models;

public partial class BorrowDetail
{
    public int Id { get; set; }

    public int BorrowId { get; set; }

    public int BookId { get; set; }

    public int Amount { get; set; }

    public int? StatusId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Borrow Borrow { get; set; } = null!;

    public virtual Status? Status { get; set; }
}
