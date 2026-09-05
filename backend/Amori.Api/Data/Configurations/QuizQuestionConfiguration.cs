using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("quiz_questions");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasColumnName("id");
        builder.Property(q => q.RelationshipId).HasColumnName("relationship_id"); // nullable = global question
        builder.Property(q => q.QuestionText).HasColumnName("question_text").HasMaxLength(1000).IsRequired();
        builder.Property(q => q.CorrectAnswer).HasColumnName("correct_answer").HasMaxLength(500).IsRequired();
        builder.Property(q => q.Options).HasColumnName("options").HasColumnType("jsonb").IsRequired();
        builder.Property(q => q.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(q => q.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(q => q.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(q => q.RelationshipId).HasDatabaseName("IX_quiz_questions_relationship_id");
        builder.HasIndex(q => q.IsActive).HasDatabaseName("IX_quiz_questions_is_active");

        builder.HasOne(q => q.Relationship)
            .WithMany()
            .HasForeignKey(q => q.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
