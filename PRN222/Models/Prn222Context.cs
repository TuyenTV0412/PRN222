using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRN222.Models;

public partial class Prn222Context : DbContext
{
    public Prn222Context()
    {
    }

    public Prn222Context(DbContextOptions<Prn222Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Borrow> Borrows { get; set; }

    public virtual DbSet<BorrowDetail> BorrowDetails { get; set; }

    public virtual DbSet<Card> Cards { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Punish> Punishes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfiguration configuration = builder.Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));


    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__Author__70DAFC141334A6E9");

            entity.ToTable("Author");

            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.AuthorName).HasMaxLength(100);
            entity.Property(e => e.Hometown).HasMaxLength(200);
            entity.Property(e => e.Image)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Book__3DE0C227998552C4");

            entity.ToTable("Book");

            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.BookName).HasMaxLength(100);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Images)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublisherId).HasColumnName("PublisherID");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Author).WithMany(p => p.Books)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Book__AuthorID__2F10007B");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Book__CategoryID__30F848ED");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Books)
                .HasForeignKey(d => d.PublisherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Book__PublisherI__300424B4");
        });

        modelBuilder.Entity<Borrow>(entity =>
        {
            entity.HasKey(e => e.BorrowId).HasName("PK__Borrow__4295F85FD9F4CC6C");

            entity.ToTable("Borrow");

            entity.Property(e => e.BorrowId).HasColumnName("BorrowID");
            entity.Property(e => e.PersonId).HasColumnName("PersonID");

            entity.HasOne(d => d.Person).WithMany(p => p.Borrows)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Borrow__PersonID__33D4B598");
        });

        modelBuilder.Entity<BorrowDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BorrowDe__3214EC07E51ECDCC");

            entity.ToTable("BorrowDetail");

            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.BorrowId).HasColumnName("BorrowID");

            entity.HasOne(d => d.Book).WithMany(p => p.BorrowDetails)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BorrowDet__BookI__37A5467C");

            entity.HasOne(d => d.Borrow).WithMany(p => p.BorrowDetails)
                .HasForeignKey(d => d.BorrowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BorrowDet__Borro__36B12243");

            entity.HasOne(d => d.Status).WithMany(p => p.BorrowDetails)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_BorrowDetail_Status");
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.CardId).HasName("PK__Card__55FECD8EE55A3807");

            entity.ToTable("Card");

            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.PersonId).HasColumnName("PersonID");

            entity.HasOne(d => d.Person).WithMany(p => p.Cards)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Card__PersonID__3E52440B");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A2B68906890");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(200);
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.PublisherId).HasName("PK__Publishe__4C657E4BD80D26A9");

            entity.ToTable("Publisher");

            entity.Property(e => e.PublisherId).HasColumnName("PublisherID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PublisherName).HasMaxLength(200);
            entity.Property(e => e.Website)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Punish>(entity =>
        {
            entity.HasKey(e => e.PunishId).HasName("PK__Punish__7FB6D512E5E89852");

            entity.ToTable("Punish");

            entity.Property(e => e.PunishId).HasColumnName("PunishID");
            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.PersonId).HasColumnName("PersonID");
            entity.Property(e => e.PunishDetail).HasMaxLength(200);

            entity.HasOne(d => d.Book).WithMany(p => p.Punishes)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Punish__BookID__3A81B327");

            entity.HasOne(d => d.Person).WithMany(p => p.Punishes)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Punish__PersonID__3B75D760");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3ACDC8E716");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Status__C8EE20633C55E584");

            entity.ToTable("Status");

            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.PersonId).HasName("PK__Users__AA2FFB8564BA2804");

            entity.Property(e => e.PersonId).HasColumnName("PersonID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Gender).HasMaxLength(6);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleID__2A4B4B5E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
