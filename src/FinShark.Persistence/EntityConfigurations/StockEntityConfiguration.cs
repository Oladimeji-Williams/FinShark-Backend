using FinShark.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinShark.Persistence.EntityConfigurations;

/// <summary>
/// Entity type configuration for Stock
/// Defines database schema and constraints using Fluent API
/// Separated for clean code organization and maintainability
/// </summary>
public static class StockEntityConfiguration
{
    /// <summary>
    /// Configures the Stock entity mapping with the database
    /// </summary>
    public static void ConfigureStock(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stock>(entity =>
        {
            // Primary Key
            entity.HasKey(e => e.Id);

            // Symbol Column
            entity.Property(e => e.Symbol)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("Stock ticker symbol (e.g., AAPL, MSFT)");

            // Company Name Column
            entity.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("Full company name");

            // Current Price Column
            entity.Property(e => e.CurrentPrice)
                .HasPrecision(18, 2)
                .HasComment("Stock price with 2 decimal places");

            // Purchase Price Column
            entity.Property(e => e.Purchase)
                .HasPrecision(18, 2)
                .HasComment("Purchase price with 2 decimal places");

            // Last Dividend Column
            entity.Property(e => e.LastDiv)
                .HasPrecision(18, 2)
                .HasComment("Last dividend amount with 2 decimal places");

            // Sector Column (stored in existing Sector column for compatibility)
            entity.Property(e => e.Sector)
                .HasColumnName("Sector")
                .HasConversion(
                    sector => sector.Value,
                    value => FinShark.Domain.ValueObjects.SectorType.From(value))
                .HasMaxLength(100)
                .HasComment("Sector sector");

            // Market Cap Column
            entity.Property(e => e.MarketCap)
                .HasPrecision(18, 2)
                .HasComment("Market capitalization");

            // Audit shadow columns
            entity.Property(e => e.Created)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Record creation timestamp");

            entity.Property(e => e.Modified)
                .HasComment("Record last update timestamp");

            entity.Property<string>("CreatedBy")
                .HasMaxLength(450)
                .HasComment("Record creator user id");

            entity.Property<bool>(nameof(Stock.IsDeleted))
                .HasDefaultValue(false)
                .HasComment("Soft delete flag");

            entity.Property<string>("ModifiedBy")
                .HasMaxLength(450)
                .HasComment("Record modifier user id");

            // Indexes for performance
            entity.HasIndex(e => e.Symbol)
                .IsUnique()
                .HasDatabaseName("IX_Stock_Symbol");
        });
    }
}
