using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class MoodCheckInConfiguration : IEntityTypeConfiguration<MoodCheckIn>
{
    public void Configure(EntityTypeBuilder<MoodCheckIn> builder)
    {
        builder.ToTable("mood_check_ins");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(m => m.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(m => m.CheckInType).HasColumnName("check_in_type").IsRequired();
        builder.Property(m => m.Mood).HasColumnName("mood").IsRequired();
        builder.Property(m => m.Intensity).HasColumnName("intensity").IsRequired();

        builder.Property(m => m.WhatHappened).HasColumnName("what_happened").HasMaxLength(2000);
        builder.Property(m => m.Feelings).HasColumnName("feelings").HasMaxLength(2000);
        builder.Property(m => m.PerceivedCause).HasColumnName("perceived_cause").HasMaxLength(1000);
        builder.Property(m => m.WhatINeed).HasColumnName("what_i_need").HasMaxLength(1000);
        builder.Property(m => m.IsSharedWithPartner).HasColumnName("is_shared_with_partner").IsRequired();

        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(m => m.UserId).HasDatabaseName("IX_mood_check_ins_user_id");
        builder.HasIndex(m => m.RelationshipId).HasDatabaseName("IX_mood_check_ins_relationship_id");
        builder.HasIndex(m => m.CreatedAt).HasDatabaseName("IX_mood_check_ins_created_at");
        builder.HasIndex(m => m.Mood).HasDatabaseName("IX_mood_check_ins_mood");

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Relationship)
            .WithMany()
            .HasForeignKey(m => m.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
