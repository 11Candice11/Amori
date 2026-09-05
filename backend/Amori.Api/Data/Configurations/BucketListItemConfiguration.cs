using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class BucketListItemConfiguration : IEntityTypeConfiguration<BucketListItem>
{
    public void Configure(EntityTypeBuilder<BucketListItem> builder)
    {
        builder.ToTable("bucket_list_items");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id");
        builder.Property(b => b.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(b => b.AddedByUserId).HasColumnName("added_by_user_id").IsRequired();
        builder.Property(b => b.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(b => b.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(b => b.Location).HasColumnName("location").HasMaxLength(300);
        builder.Property(b => b.Category).HasColumnName("category").IsRequired();
        builder.Property(b => b.TargetDate).HasColumnName("target_date");
        builder.Property(b => b.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(b => b.IsFavorite).HasColumnName("is_favorite").IsRequired();
        builder.Property(b => b.IsCompleted).HasColumnName("is_completed").IsRequired();
        builder.Property(b => b.CompletedAt).HasColumnName("completed_at");
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(b => b.RelationshipId).HasDatabaseName("IX_bucket_list_items_relationship_id");

        builder.HasOne(b => b.Relationship)
            .WithMany()
            .HasForeignKey(b => b.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.AddedBy)
            .WithMany()
            .HasForeignKey(b => b.AddedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
