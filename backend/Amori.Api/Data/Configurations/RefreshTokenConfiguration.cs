using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(t => t.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
        builder.Property(t => t.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(t => t.IsRevoked).HasColumnName("is_revoked").IsRequired();
        builder.Property(t => t.ReplacedByToken).HasColumnName("replaced_by_token").HasMaxLength(500);
        builder.Property(t => t.RevokedReason).HasColumnName("revoked_reason").HasMaxLength(200);
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(t => t.Token).IsUnique().HasDatabaseName("IX_refresh_tokens_token");
        builder.HasIndex(t => t.UserId).HasDatabaseName("IX_refresh_tokens_user_id");
        builder.HasIndex(t => t.ExpiresAt).HasDatabaseName("IX_refresh_tokens_expires_at");

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
