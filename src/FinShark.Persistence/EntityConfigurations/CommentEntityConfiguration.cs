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

            // User ID (Foreign Key)
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasComment("Reference to ApplicationUser");

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

            // Audit shadow columns
            entity.Property(e => e.Created)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Record creation timestamp");

            entity.Property(e => e.Modified)
                .HasComment("Record last update timestamp");

            entity.Property<bool>(nameof(Comment.IsDeleted))
                .HasDefaultValue(false)
                .HasComment("Soft delete flag");

            entity.Property<string>("CreatedBy")
                .HasMaxLength(450)
                .HasComment("Record creator user id");

            entity.Property<string>("ModifiedBy")
                .HasMaxLength(450)
                .HasComment("Record modifier user id");

            // Foreign Key Relationship
            entity.HasOne(c => c.Stock)
                .WithMany(s => s.Comments)
                .HasForeignKey(e => e.StockId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Comments_Stocks");

            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Comments_AspNetUsers_UserId");

            // Indexes for performance
            entity.HasIndex(e => e.StockId)
                .HasDatabaseName("IX_Comment_StockId");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Comment_UserId");

            entity.HasIndex(e => e.Created)
                .HasDatabaseName("IX_Comment_Created");

            // Check constraints
            entity.ToTable(t => t.HasCheckConstraint("CK_Comment_Rating", "[Rating] >= 1 AND [Rating] <= 5"));
        });
    }
}
