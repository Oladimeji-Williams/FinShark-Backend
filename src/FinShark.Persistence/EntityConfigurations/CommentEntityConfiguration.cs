using FinShark.Domain.Entities;
using FinShark.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FinShark.Persistence.EntityConfigurations;

/// <summary>
/// Entity type configuration for Comment
/// Defines database schema and constraints using Fluent API
/// </summary>
public static class CommentEntityConfiguration
{
    /// <summary>
    /// Configures the Comment entity mapping with the database
    /// </summary>
    public static void ConfigureComment(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            // Primary Key
            entity.HasKey(e => e.Id);

            // Stock ID (Foreign Key)
            entity.Property(e => e.StockId)
                .IsRequired()
                .HasComment("Reference to Stock");

            // Title Column
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("Comment title");

            // Content Column
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)")
                .HasComment("Comment content");

            // Rating Column
            entity.Property(e => e.Rating)
                .HasConversion(
                    rating => rating.Value,
                    value => Rating.From(value))
                .IsRequired()
                .HasComment("Rating from 1 to 5");

            // Audit Columns
            entity.Property(e => e.Created)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Record creation timestamp");

            entity.Property(e => e.Modified)
                .HasComment("Record last update timestamp");

            // Foreign Key Relationship
            entity.HasOne(c => c.Stock)
                .WithMany(s => s.Comments)
                .HasForeignKey(e => e.StockId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Comments_Stocks");

            // Indexes for performance
            entity.HasIndex(e => e.StockId)
                .HasDatabaseName("IX_Comment_StockId");

            entity.HasIndex(e => e.Created)
                .HasDatabaseName("IX_Comment_Created");

            // Check constraints
            entity.ToTable(t => t.HasCheckConstraint("CK_Comment_Rating", "[Rating] >= 1 AND [Rating] <= 5"));
        });
    }
}
