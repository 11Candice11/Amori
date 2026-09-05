using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class RelationshipIncidentResponseConfiguration : IEntityTypeConfiguration<RelationshipIncidentResponse>
{
    public void Configure(EntityTypeBuilder<RelationshipIncidentResponse> builder)
    {
        builder.ToTable("relationship_incident_responses");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.IncidentId).HasColumnName("incident_id").IsRequired();
        builder.Property(r => r.AuthorUserId).HasColumnName("author_user_id").IsRequired();

        builder.Property(r => r.Message)
            .HasColumnName("message")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(r => r.IncidentId).HasDatabaseName("IX_incident_responses_incident_id");

        builder.HasOne(r => r.Incident)
            .WithMany(i => i.Responses)
            .HasForeignKey(r => r.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Author)
            .WithMany()
            .HasForeignKey(r => r.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
