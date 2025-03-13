using System;
using System.Collections.Generic;

namespace PRN222.Models;

public partial class Author
{
    public int AuthorId { get; set; }

    public string AuthorName { get; set; } = null!;

    public string? Hometown { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? DateOfDeath { get; set; }

    public string? Image { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
