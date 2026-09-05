using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class SplittingSessionConfiguration : IEntityTypeConfiguration<SplittingSession>
{
    public void Configure(EntityTypeBuilder<SplittingSession> builder)
    {
        builder.ToTable("splitting_sessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(s => s.RelationshipId).HasColumnName("relationship_id").IsRequired();

        // JSON arrays stored as jsonb
        builder.Property(s => s.FeelingsSelected)
            .HasColumnName("feelings_selected")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(s => s.ActionsTaken)
            .HasColumnName("actions_taken")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(s => s.Trigger).HasColumnName("trigger").HasMaxLength(500);
        builder.Property(s => s.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(s => s.WhatINeed).HasColumnName("what_i_need").HasMaxLength(1000);
        builder.Property(s => s.RecommendedSupportType).HasColumnName("recommended_support_type");
        builder.Property(s => s.InitialMood).HasColumnName("initial_mood");
        builder.Property(s => s.FinalMood).HasColumnName("final_mood");
        builder.Property(s => s.Status).HasColumnName("status").IsRequired();
        builder.Property(s => s.CompletedAt).HasColumnName("completed_at");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(s => s.UserId).HasDatabaseName("IX_splitting_sessions_user_id");
        builder.HasIndex(s => s.RelationshipId).HasDatabaseName("IX_splitting_sessions_relationship_id");
        builder.HasIndex(s => s.Status).HasDatabaseName("IX_splitting_sessions_status");
        builder.HasIndex(s => s.CreatedAt).HasDatabaseName("IX_splitting_sessions_created_at");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Relationship)
            .WithMany()
            .HasForeignKey(s => s.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Answers)
            .WithOne(a => a.Session)
            .HasForeignKey(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
