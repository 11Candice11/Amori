using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class ImportantDateConfiguration : IEntityTypeConfiguration<ImportantDate>
{
    public void Configure(EntityTypeBuilder<ImportantDate> builder)
    {
        builder.ToTable("important_dates");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(i => i.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(i => i.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(i => i.Date).HasColumnName("date").IsRequired();
        builder.Property(i => i.IsRecurring).HasColumnName("is_recurring").IsRequired();
        builder.Property(i => i.ReminderEnabled).HasColumnName("reminder_enabled").IsRequired();
        builder.Property(i => i.ReminderDaysBefore).HasColumnName("reminder_days_before");
        builder.Property(i => i.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(i => i.ImageKey).HasColumnName("image_key").HasMaxLength(500);
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(i => i.RelationshipId).HasDatabaseName("IX_important_dates_relationship_id");
        builder.HasIndex(i => i.Date).HasDatabaseName("IX_important_dates_date");

        builder.HasOne(i => i.Relationship)
            .WithMany()
            .HasForeignKey(i => i.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.CreatedBy)
            .WithMany()
            .HasForeignKey(i => i.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
