using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class DateIdeaConfiguration : IEntityTypeConfiguration<DateIdea>
{
    public void Configure(EntityTypeBuilder<DateIdea> builder)
    {
        builder.ToTable("date_ideas");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(d => d.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(d => d.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(d => d.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(d => d.Category).HasColumnName("category").IsRequired();
        builder.Property(d => d.Location).HasColumnName("location").HasMaxLength(300);
        builder.Property(d => d.EstimatedCost).HasColumnName("estimated_cost").HasColumnType("numeric(10,2)");
        builder.Property(d => d.DurationMinutes).HasColumnName("duration_minutes");
        builder.Property(d => d.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(d => d.IsFavorite).HasColumnName("is_favorite").IsRequired();
        builder.Property(d => d.IsCompleted).HasColumnName("is_completed").IsRequired();
        builder.Property(d => d.CompletedAt).HasColumnName("completed_at");
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(d => d.RelationshipId).HasDatabaseName("IX_date_ideas_relationship_id");

        builder.HasOne(d => d.Relationship)
            .WithMany()
            .HasForeignKey(d => d.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.CreatedBy)
            .WithMany()
            .HasForeignKey(d => d.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
