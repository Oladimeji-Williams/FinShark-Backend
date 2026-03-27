using FinShark.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinShark.Persistence.EntityConfigurations;

/// <summary>
/// Entity configuration for portfolio items linking users to stocks
/// </summary>
public static class PortfolioItemEntityConfiguration
{
    public static void ConfigurePortfolioItem(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PortfolioItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450)
                .HasComment("Reference to ApplicationUser");

            entity.Property(e => e.StockId)
                .IsRequired()
                .HasComment("Reference to Stock entity");

            entity.Property(e => e.Created)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("When this stock was added to user portfolio");

            entity.Property<string>("CreatedBy")
                .HasMaxLength(450)
                .HasComment("User who added this portfolio item");

            entity.Property<bool>(nameof(PortfolioItem.IsDeleted))
                .HasDefaultValue(false)
                .HasComment("Soft delete flag for portfolio item");

            entity.Property(e => e.Modified)
                .HasComment("When this stock portfolio item was modified");

            entity.Property<string>("ModifiedBy")
                .HasMaxLength(450)
                .HasComment("User who modified this portfolio item");

            entity.HasIndex(e => new { e.UserId, e.StockId })
                .IsUnique()
                .HasDatabaseName("IX_PortfolioItem_UserId_StockId");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Portfolio)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Stock)
                .WithMany(s => s.PortfolioItems)
                .HasForeignKey(e => e.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
