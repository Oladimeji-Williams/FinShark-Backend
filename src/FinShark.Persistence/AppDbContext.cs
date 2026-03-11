using FinShark.Domain.Entities;
using FinShark.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace FinShark.Persistence;

/// <summary>
/// Application DbContext for Entity Framework Core
/// Manages database operations and orchestrates entity configurations
/// Automatically handles audit timestamps (Created, Modified)
/// </summary>
public sealed class AppDbContext : DbContext
{
    public required DbSet<Stock> Stocks { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities using dedicated configuration classes
        modelBuilder.ConfigureStock();
    }

    /// <summary>
    /// Overrides SaveChangesAsync to automatically update the Modified timestamp
    /// for any modified entities that inherit from BaseEntity
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update the Modified timestamp for all modified entities
        var entries = ChangeTracker.Entries<BaseEntity>();
        
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Modified = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}