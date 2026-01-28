using BookLibrary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Infrastructure.Data;

public class BookLibraryDbContext : DbContext
{
    public BookLibraryDbContext(DbContextOptions<BookLibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Place> Places => Set<Place>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Place>(entity =>
        {
            entity.ToTable("places");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descr).HasColumnName("descr").HasMaxLength(500);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("books");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Isbn).HasColumnName("isbn").HasMaxLength(50);
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(500);
            entity.Property(e => e.Author).HasColumnName("author").HasMaxLength(300);
            entity.Property(e => e.Publisher).HasColumnName("publisher").HasMaxLength(300);
            entity.Property(e => e.PublishedYear).HasColumnName("published_year").HasMaxLength(10);
            entity.Property(e => e.PageCount).HasColumnName("pagecount");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");
            entity.Property(e => e.ApiInfo).HasColumnName("api_info").HasColumnType("jsonb");

            entity.HasOne(e => e.Place)
                .WithMany(p => p.Books)
                .HasForeignKey(e => e.PlaceId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
