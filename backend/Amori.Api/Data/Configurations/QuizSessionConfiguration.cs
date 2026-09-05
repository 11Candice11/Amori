using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class QuizSessionConfiguration : IEntityTypeConfiguration<QuizSession>
{
    public void Configure(EntityTypeBuilder<QuizSession> builder)
    {
        builder.ToTable("quiz_sessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(s => s.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(s => s.Score).HasColumnName("score").IsRequired();
        builder.Property(s => s.TotalQuestions).HasColumnName("total_questions").IsRequired();
        builder.Property(s => s.CompletedAt).HasColumnName("completed_at");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(s => s.RelationshipId).HasDatabaseName("IX_quiz_sessions_relationship_id");
        builder.HasIndex(s => s.UserId).HasDatabaseName("IX_quiz_sessions_user_id");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Relationship)
            .WithMany()
            .HasForeignKey(s => s.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Answers)
            .WithOne(a => a.QuizSession)
            .HasForeignKey(a => a.QuizSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
