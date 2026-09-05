using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class WouldYouRatherQuestionConfiguration : IEntityTypeConfiguration<WouldYouRatherQuestion>
{
    public void Configure(EntityTypeBuilder<WouldYouRatherQuestion> builder)
    {
        builder.ToTable("would_you_rather_questions");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasColumnName("id");
        builder.Property(q => q.OptionA).HasColumnName("option_a").HasMaxLength(500).IsRequired();
        builder.Property(q => q.OptionB).HasColumnName("option_b").HasMaxLength(500).IsRequired();
        builder.Property(q => q.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(q => q.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(q => q.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(q => q.IsActive).HasDatabaseName("IX_would_you_rather_questions_is_active");

        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
