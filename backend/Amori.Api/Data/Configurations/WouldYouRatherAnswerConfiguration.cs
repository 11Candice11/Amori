using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class WouldYouRatherAnswerConfiguration : IEntityTypeConfiguration<WouldYouRatherAnswer>
{
    public void Configure(EntityTypeBuilder<WouldYouRatherAnswer> builder)
    {
        builder.ToTable("would_you_rather_answers");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.QuestionId).HasColumnName("question_id").IsRequired();
        builder.Property(a => a.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(a => a.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(a => a.ChoseOptionA).HasColumnName("chose_option_a").IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => a.QuestionId).HasDatabaseName("IX_would_you_rather_answers_question_id");
        builder.HasIndex(a => a.RelationshipId).HasDatabaseName("IX_would_you_rather_answers_relationship_id");

        builder.HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Relationship)
            .WithMany()
            .HasForeignKey(a => a.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
