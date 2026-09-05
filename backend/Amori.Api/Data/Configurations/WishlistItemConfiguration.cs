using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("wishlist_items");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(w => w.AddedByUserId).HasColumnName("added_by_user_id").IsRequired();
        builder.Property(w => w.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(w => w.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(w => w.ImageKey).HasColumnName("image_key").HasMaxLength(500);
        builder.Property(w => w.Price).HasColumnName("price").HasColumnType("numeric(10,2)");
        builder.Property(w => w.Url).HasColumnName("url").HasMaxLength(2048);
        builder.Property(w => w.Priority).HasColumnName("priority").IsRequired();
        builder.Property(w => w.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(w => w.IsPurchased).HasColumnName("is_purchased").IsRequired();
        builder.Property(w => w.IsFavorite).HasColumnName("is_favorite").IsRequired();
        builder.Property(w => w.PurchasedAt).HasColumnName("purchased_at");
        builder.Property(w => w.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(w => w.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(w => w.RelationshipId).HasDatabaseName("IX_wishlist_items_relationship_id");

        builder.HasOne(w => w.Relationship)
            .WithMany()
            .HasForeignKey(w => w.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.AddedBy)
            .WithMany()
            .HasForeignKey(w => w.AddedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
