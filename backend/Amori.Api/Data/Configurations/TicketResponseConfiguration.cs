using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class TicketResponseConfiguration : IEntityTypeConfiguration<TicketResponse>
{
    public void Configure(EntityTypeBuilder<TicketResponse> builder)
    {
        builder.ToTable("ticket_responses");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.TicketId).HasColumnName("ticket_id").IsRequired();
        builder.Property(r => r.RespondedByUserId).HasColumnName("responded_by_user_id").IsRequired();
        builder.Property(r => r.Content).HasColumnName("content").HasMaxLength(4000).IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(r => r.TicketId).HasDatabaseName("IX_ticket_responses_ticket_id");

        builder.HasOne(r => r.Ticket)
            .WithMany(t => t.Responses)
            .HasForeignKey(r => r.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.RespondedBy)
            .WithMany()
            .HasForeignKey(r => r.RespondedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
