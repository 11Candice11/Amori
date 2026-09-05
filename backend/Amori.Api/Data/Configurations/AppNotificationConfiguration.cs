using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class AppNotificationConfiguration : IEntityTypeConfiguration<AppNotification>
{
    public void Configure(EntityTypeBuilder<AppNotification> builder)
    {
        builder.ToTable("app_notifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(n => n.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(n => n.Body).HasColumnName("body").HasMaxLength(1000).IsRequired();
        builder.Property(n => n.NotificationType).HasColumnName("notification_type").HasMaxLength(100);
        builder.Property(n => n.ReferenceId).HasColumnName("reference_id").HasMaxLength(100);
        builder.Property(n => n.IsRead).HasColumnName("is_read").IsRequired();
        builder.Property(n => n.ReadAt).HasColumnName("read_at");
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(n => n.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(n => n.UserId).HasDatabaseName("IX_app_notifications_user_id");
        builder.HasIndex(n => n.IsRead).HasDatabaseName("IX_app_notifications_is_read");
        builder.HasIndex(n => n.CreatedAt).HasDatabaseName("IX_app_notifications_created_at");

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
