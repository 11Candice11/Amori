using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class RelationshipInvitationConfiguration : IEntityTypeConfiguration<RelationshipInvitation>
{
    public void Configure(EntityTypeBuilder<RelationshipInvitation> builder)
    {
        builder.ToTable("relationship_invitations");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(i => i.InvitedByUserId).HasColumnName("invited_by_user_id").IsRequired();
        builder.Property(i => i.InviteeEmail).HasColumnName("invitee_email").HasMaxLength(256);
        builder.Property(i => i.InviteCode).HasColumnName("invite_code").HasMaxLength(50).IsRequired();
        builder.Property(i => i.Status).HasColumnName("status").IsRequired();
        builder.Property(i => i.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(i => i.RespondedAt).HasColumnName("responded_at");
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(i => i.InviteCode).IsUnique().HasDatabaseName("IX_relationship_invitations_invite_code");
        builder.HasIndex(i => i.RelationshipId).HasDatabaseName("IX_relationship_invitations_relationship_id");

        builder.HasOne(i => i.Relationship)
            .WithMany()
            .HasForeignKey(i => i.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.InvitedBy)
            .WithMany()
            .HasForeignKey(i => i.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
