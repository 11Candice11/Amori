using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("games");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id");
        builder.Property(g => g.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(g => g.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(g => g.Type).HasColumnName("type").IsRequired();
        builder.Property(g => g.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(g => g.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(g => g.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(g => g.IsActive).HasDatabaseName("IX_games_is_active");
        builder.HasIndex(g => g.Type).HasDatabaseName("IX_games_type");

        builder.HasMany(g => g.Sessions)
            .WithOne(s => s.Game)
            .HasForeignKey(s => s.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.Scores)
            .WithOne(s => s.Game)
            .HasForeignKey(s => s.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
