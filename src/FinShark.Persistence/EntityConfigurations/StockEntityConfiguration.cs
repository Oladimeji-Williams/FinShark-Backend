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
                .HasMaxLength(10)
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

            // Industry Column
            entity.Property(e => e.Industry)
                .HasConversion(
                    industry => industry.Value,
                    value => FinShark.Domain.ValueObjects.IndustryType.From(value))
                .HasMaxLength(100)
                .HasComment("Industry sector");

            // Market Cap Column
            entity.Property(e => e.MarketCap)
                .HasPrecision(18, 2)
                .HasComment("Market capitalization");

            // Audit shadow columns
            entity.Property<DateTime>("Created")
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Record creation timestamp");

            entity.Property<DateTime?>("Modified")
                .HasComment("Record last update timestamp");

            entity.Property<string>("CreatedBy")
                .HasMaxLength(450)
                .HasComment("Record creator user id");

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
