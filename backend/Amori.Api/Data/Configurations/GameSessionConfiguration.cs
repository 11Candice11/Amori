using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("game_sessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.GameId).HasColumnName("game_id").IsRequired();
        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(s => s.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").IsRequired();
        builder.Property(s => s.Score).HasColumnName("score");
        builder.Property(s => s.CompletedAt).HasColumnName("completed_at");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(s => s.RelationshipId).HasDatabaseName("IX_game_sessions_relationship_id");
        builder.HasIndex(s => s.UserId).HasDatabaseName("IX_game_sessions_user_id");
        builder.HasIndex(s => s.CreatedAt).HasDatabaseName("IX_game_sessions_created_at");

        builder.HasOne(s => s.Game)
            .WithMany(g => g.Sessions)
            .HasForeignKey(s => s.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Relationship)
            .WithMany()
            .HasForeignKey(s => s.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
