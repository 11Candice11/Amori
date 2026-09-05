using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class SplittingQuestionConfiguration : IEntityTypeConfiguration<SplittingQuestion>
{
    public void Configure(EntityTypeBuilder<SplittingQuestion> builder)
    {
        builder.ToTable("splitting_questions");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasColumnName("id");
        builder.Property(q => q.Question).HasColumnName("question").HasMaxLength(1000).IsRequired();
        builder.Property(q => q.QuestionType).HasColumnName("question_type").HasMaxLength(50);
        builder.Property(q => q.DisplayOrder).HasColumnName("display_order").IsRequired();
        builder.Property(q => q.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(q => q.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(q => q.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(q => q.DisplayOrder).HasDatabaseName("IX_splitting_questions_display_order");
        builder.HasIndex(q => q.IsActive).HasDatabaseName("IX_splitting_questions_is_active");

        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
