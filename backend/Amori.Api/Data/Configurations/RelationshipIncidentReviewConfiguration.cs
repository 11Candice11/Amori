using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class RelationshipIncidentReviewConfiguration : IEntityTypeConfiguration<RelationshipIncidentReview>
{
    public void Configure(EntityTypeBuilder<RelationshipIncidentReview> builder)
    {
        builder.ToTable("relationship_incident_reviews");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.IncidentId).HasColumnName("incident_id").IsRequired();
        builder.Property(r => r.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();

        builder.Property(r => r.WhatWentWell).HasColumnName("what_went_well").HasMaxLength(2000);
        builder.Property(r => r.WhatCouldImprove).HasColumnName("what_could_improve").HasMaxLength(2000);
        builder.Property(r => r.FutureAction).HasColumnName("future_action").HasMaxLength(2000);

        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(r => r.IncidentId).HasDatabaseName("IX_incident_reviews_incident_id");

        builder.HasOne(r => r.Incident)
            .WithMany(i => i.Reviews)
            .HasForeignKey(r => r.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
