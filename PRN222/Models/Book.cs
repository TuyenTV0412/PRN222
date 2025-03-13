using System;
using System.Collections.Generic;

namespace PRN222.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string BookName { get; set; } = null!;

    public string? Images { get; set; }

    public int AuthorId { get; set; }

    public int PublisherId { get; set; }

    public int CategoryId { get; set; }

    public int PublishingYear { get; set; }

    public string? Description { get; set; }

    public int? Quantity { get; set; }

    public virtual Author Author { get; set; } = null!;

    public virtual ICollection<BorrowDetail> BorrowDetails { get; set; } = new List<BorrowDetail>();

    public virtual Category Category { get; set; } = null!;

    public virtual Publisher Publisher { get; set; } = null!;

    public virtual ICollection<Punish> Punishes { get; set; } = new List<Punish>();
}
