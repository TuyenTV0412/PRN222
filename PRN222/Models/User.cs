using System;
using System.Collections.Generic;

namespace PRN222.Models;

public partial class User
{
    public int PersonId { get; set; }

    public string? Name { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? StartDate { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string Username { get; set; } = null!;

    public string? Password { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();

    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();

    public virtual ICollection<Punish> Punishes { get; set; } = new List<Punish>();

    public virtual Role Role { get; set; } = null!;
}
