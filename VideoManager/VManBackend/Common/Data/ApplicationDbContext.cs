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
    public DbSet<Item> Items => Set<Item>();
    public DbSet<SyncJob> SyncJobs => Set<SyncJob>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionItem> CollectionItems => Set<CollectionItem>();

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

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProviderName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProviderItemId).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastSyncedAt).IsRequired();

            // Unique constraint on provider + item ID
            entity.HasIndex(e => new { e.ProviderName, e.ProviderItemId })
                .IsUnique()
                .HasDatabaseName("IX_Items_Provider_ItemId");

            // Index for filtering by provider
            entity.HasIndex(e => e.ProviderName)
                .HasDatabaseName("IX_Items_Provider");

            // Index for sorting by created date
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Items_CreatedAt");
        });

        modelBuilder.Entity<SyncJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProviderName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.StartedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);

            // Index for finding latest sync job by provider
            entity.HasIndex(e => new { e.ProviderName, e.StartedAt })
                .HasDatabaseName("IX_SyncJobs_Provider_StartedAt");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasMany(e => e.CollectionItems)
                .WithOne(e => e.Collection)
                .HasForeignKey(e => e.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CollectionItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CollectionId).IsRequired();
            entity.Property(e => e.ProviderName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProviderItemId).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            // NOTE: CollectionItems reference Items table by ProviderName and ProviderItemId strings
            // without a foreign key constraint. This is intentional to support cross-provider items.
            // However, this means collection items can become orphaned if items are deleted from
            // the Items table (e.g., during sync cleanup). Application logic should handle cleanup
            // of orphaned collection items or validate item existence before export operations.

            // Composite unique index to prevent duplicate items in same collection
            entity.HasIndex(e => new { e.CollectionId, e.ProviderName, e.ProviderItemId })
                .IsUnique()
                .HasDatabaseName("IX_CollectionItems_Unique");

            // Index for ordering items within a collection
            entity.HasIndex(e => new { e.CollectionId, e.Order })
                .HasDatabaseName("IX_CollectionItems_Collection_Order");
        });
    }
}
