using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Models;

namespace VManBackend.Common.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ImmichAsset> ImmichAssets => Set<ImmichAsset>();
    public DbSet<ImmichExifData> ImmichExifData => Set<ImmichExifData>();
    public DbSet<SyncHistory> SyncHistories => Set<SyncHistory>();

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

        modelBuilder.Entity<ImmichAsset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginalFileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OriginalPath).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.AssetType).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastSyncedAt).IsRequired();
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(2000);
            
            entity.HasIndex(e => e.AssetType);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.LastSyncedAt);
            
            entity.HasOne(e => e.ExifData)
                .WithOne(e => e.Asset)
                .HasForeignKey<ImmichExifData>(e => e.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ImmichExifData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssetId).IsRequired();
            entity.Property(e => e.Make).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.LensModel).HasMaxLength(100);
            entity.Property(e => e.ExposureTime).HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            
            entity.HasIndex(e => e.AssetId).IsUnique();
        });

        modelBuilder.Entity<SyncHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.AssetTypeFilter).HasMaxLength(50);
            
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.Status);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
