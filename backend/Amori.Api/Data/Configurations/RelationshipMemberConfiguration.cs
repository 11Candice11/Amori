using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class RelationshipMemberConfiguration : IEntityTypeConfiguration<RelationshipMember>
{
    public void Configure(EntityTypeBuilder<RelationshipMember> builder)
    {
        builder.ToTable("relationship_members");

        builder.HasKey(rm => rm.Id);

        builder.Property(rm => rm.Id)
            .HasColumnName("id");

        builder.Property(rm => rm.RelationshipId)
            .HasColumnName("relationship_id")
            .IsRequired();

        builder.Property(rm => rm.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(rm => rm.Role)
            .HasColumnName("role")
            .IsRequired();

        builder.Property(rm => rm.InviteStatus)
            .HasColumnName("invite_status")
            .IsRequired();

        builder.Property(rm => rm.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(rm => rm.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(rm => new { rm.RelationshipId, rm.UserId })
            .IsUnique();

        builder.HasOne(rm => rm.Relationship)
            .WithMany(r => r.Members)
            .HasForeignKey(rm => rm.RelationshipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rm => rm.User)
            .WithMany(u => u.RelationshipMemberships)
            .HasForeignKey(rm => rm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
