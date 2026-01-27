using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Models;

namespace VManBackend.Common.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ItemTag> ItemTags => Set<ItemTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasMany(e => e.ItemTags)
                .WithOne(e => e.Tag)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TagId).IsRequired();
            entity.Property(e => e.ProviderName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProviderItemId).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            
            // Composite unique index to prevent duplicate tags on same item
            entity.HasIndex(e => new { e.TagId, e.ProviderName, e.ProviderItemId })
                .IsUnique()
                .HasDatabaseName("IX_ItemTags_Unique");
            
            // Index for fast lookups by provider + item
            entity.HasIndex(e => new { e.ProviderName, e.ProviderItemId })
                .HasDatabaseName("IX_ItemTags_Provider_Item");
            
            // Index for fast lookups by tag
            entity.HasIndex(e => e.TagId)
                .HasDatabaseName("IX_ItemTags_TagId");
        });
    }
}
