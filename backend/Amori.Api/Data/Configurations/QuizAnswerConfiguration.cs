using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
{
    public void Configure(EntityTypeBuilder<QuizAnswer> builder)
    {
        builder.ToTable("quiz_answers");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.QuizSessionId).HasColumnName("quiz_session_id").IsRequired();
        builder.Property(a => a.QuestionId).HasColumnName("question_id").IsRequired();
        builder.Property(a => a.AnswerGiven).HasColumnName("answer_given").HasMaxLength(500).IsRequired();
        builder.Property(a => a.IsCorrect).HasColumnName("is_correct").IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => a.QuizSessionId).HasDatabaseName("IX_quiz_answers_quiz_session_id");

        builder.HasOne(a => a.QuizSession)
            .WithMany(s => s.Answers)
            .HasForeignKey(a => a.QuizSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
