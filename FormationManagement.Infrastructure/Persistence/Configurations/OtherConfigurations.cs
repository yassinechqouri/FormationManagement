using FormationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormationManagement.Infrastructure.Persistence.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Modules");
        builder.Property(m => m.Title).IsRequired().HasMaxLength(250);
        builder.Property(m => m.Description).HasMaxLength(2000);

        builder.HasMany(m => m.Lessons)
            .WithOne(l => l.Module)
            .HasForeignKey(l => l.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons");
        builder.Property(l => l.Title).IsRequired().HasMaxLength(250);
        builder.Property(l => l.VideoUrl).HasMaxLength(1000);
        builder.Property(l => l.DocumentUrl).HasMaxLength(1000);

        builder.HasMany(l => l.Exercises)
            .WithOne(e => e.Lesson)
            .HasForeignKey(e => e.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Quizzes)
            .WithOne(q => q.Lesson)
            .HasForeignKey(q => q.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");
        builder.Property(e => e.Title).IsRequired().HasMaxLength(250);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(4000);
    }
}

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quizzes");
        builder.Property(q => q.Title).IsRequired().HasMaxLength(250);

        builder.HasMany(q => q.Questions)
            .WithOne(qs => qs.Quiz)
            .HasForeignKey(qs => qs.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");
        builder.Property(q => q.QuestionText).IsRequired().HasMaxLength(1000);

        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("Answers");
        builder.Property(a => a.Text).IsRequired().HasMaxLength(500);
    }
}

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.Property(e => e.UserId).IsRequired().HasMaxLength(450);
        builder.Property(e => e.Progress).HasColumnType("decimal(5,2)");

        // A learner can only enroll once in the same course.
        builder.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
    }
}
