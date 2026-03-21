using System.Linq;
using FinShark.Domain.Entities;
using FinShark.Persistence.EntityConfigurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinShark.Persistence;

/// <summary>
/// Application DbContext for Entity Framework Core
/// Manages database operations and orchestrates entity configurations
/// Automatically handles audit timestamps through shadow properties
/// </summary>
public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Stock> Stocks { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<PortfolioItem> PortfolioItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure audit shadow properties on all BaseEntity derived types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            if (entityType.FindProperty("CreatedBy") == null)
            {
                entityType.AddProperty("CreatedBy", typeof(string));
            }
            if (entityType.FindProperty("ModifiedBy") == null)
            {
                entityType.AddProperty("ModifiedBy", typeof(string));
            }
        }

        // Configure entities using dedicated configuration classes
        modelBuilder.ConfigureStock();
        modelBuilder.ConfigureComment();
        modelBuilder.ConfigurePortfolioItem();

        // Soft delete filters for all entities with IsDeleted flag
        modelBuilder.Entity<Stock>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Comment>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<PortfolioItem>().HasQueryFilter(p => !p.IsDeleted);

        // Ensure Email and UserName are unique
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("EmailIndex");
    }
}
