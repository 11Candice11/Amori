using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class SplittingAnswerConfiguration : IEntityTypeConfiguration<SplittingAnswer>
{
    public void Configure(EntityTypeBuilder<SplittingAnswer> builder)
    {
        builder.ToTable("splitting_answers");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.SessionId).HasColumnName("session_id").IsRequired();
        builder.Property(a => a.QuestionId).HasColumnName("question_id").IsRequired();
        builder.Property(a => a.Answer).HasColumnName("answer").HasMaxLength(4000).IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => a.SessionId).HasDatabaseName("IX_splitting_answers_session_id");

        builder.HasOne(a => a.Session)
            .WithMany(s => s.Answers)
            .HasForeignKey(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
