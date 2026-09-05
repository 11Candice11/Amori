using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class DeviceRegistrationConfiguration : IEntityTypeConfiguration<DeviceRegistration>
{
    public void Configure(EntityTypeBuilder<DeviceRegistration> builder)
    {
        builder.ToTable("device_registrations");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(d => d.DeviceToken).HasColumnName("device_token").HasMaxLength(500).IsRequired();
        builder.Property(d => d.Platform).HasColumnName("platform").IsRequired();
        builder.Property(d => d.DeviceIdentifier).HasColumnName("device_identifier").HasMaxLength(200);
        builder.Property(d => d.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(d => d.LastSeenAt).HasColumnName("last_seen_at");
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(d => d.UserId).HasDatabaseName("IX_device_registrations_user_id");
        builder.HasIndex(d => d.DeviceToken).HasDatabaseName("IX_device_registrations_device_token");
        builder.HasIndex(d => d.IsActive).HasDatabaseName("IX_device_registrations_is_active");

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
