using System;
using System.Collections.Generic;

namespace PRN222.Models;

public partial class Publisher
{
    public int PublisherId { get; set; }

    public string PublisherName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Website { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
