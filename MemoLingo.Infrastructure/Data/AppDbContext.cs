using MemoLingo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Language> Languages => Set<Language>();

        public DbSet<LanguageProgress> LanguageProgresses => Set<LanguageProgress>();

        public DbSet<Word> Words => Set<Word>();

        public DbSet<Sentence> Sentences => Set<Sentence>();

        public DbSet<SentenceWord> SentenceWords => Set<SentenceWord>();

        public DbSet<Course> Courses => Set<Course>();

        public DbSet<Lesson> Lessons => Set<Lesson>();

        public DbSet<LessonWord> LessonWords => Set<LessonWord>();

        public DbSet<StudySession> StudySessions => Set<StudySession>();

        public DbSet<ExerciseAttempt> ExerciseAttempts => Set<ExerciseAttempt>();

        public DbSet<WordPerformance> WordPerformances => Set<WordPerformance>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Name).IsRequired().HasMaxLength(150);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);

                entity.HasOne(u => u.NativeLanguage)
                    .WithMany()
                    .HasForeignKey(u => u.NativeLanguageId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.Property(l => l.Name).IsRequired().HasMaxLength(50);
                entity.Property(l => l.Code).IsRequired().HasMaxLength(10);
                entity.HasIndex(l => l.Code).IsUnique();
            });

            modelBuilder.Entity<LanguageProgress>(entity =>
            {
                entity.HasOne(lp => lp.User)
                    .WithMany(u => u.LanguageProgresses)
                    .HasForeignKey(lp => lp.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(lp => lp.Language)
                    .WithMany()
                    .HasForeignKey(lp => lp.LanguageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(lp => new { lp.UserId, lp.LanguageId }).IsUnique();
            });

            modelBuilder.Entity<Word>(entity =>
            {
                entity.Property(w => w.Text).IsRequired().HasMaxLength(100);
                entity.Property(w => w.Translation).IsRequired().HasMaxLength(200);

                entity.HasOne(w => w.Language)
                    .WithMany()
                    .HasForeignKey(w => w.LanguageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(w => new { w.LanguageId, w.Text }).IsUnique();
                entity.HasIndex(w => new { w.LanguageId, w.CefrLevel });
            });

            modelBuilder.Entity<Sentence>(entity =>
            {
                entity.Property(s => s.Text).IsRequired().HasMaxLength(500);
                entity.Property(s => s.Translation).IsRequired().HasMaxLength(500);

                entity.HasOne(s => s.Language)
                    .WithMany()
                    .HasForeignKey(s => s.LanguageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(s => new { s.LanguageId, s.CefrLevel });
            });

            modelBuilder.Entity<SentenceWord>(entity =>
            {
                entity.HasOne(sw => sw.Sentence)
                    .WithMany(s => s.SentenceWords)
                    .HasForeignKey(sw => sw.SentenceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sw => sw.Word)
                    .WithMany(w => w.SentenceWords)
                    .HasForeignKey(sw => sw.WordId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(sw => new { sw.SentenceId, sw.WordId }).IsUnique();
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
                entity.Property(c => c.Description).HasMaxLength(500);

                entity.HasOne(c => c.Language)
                    .WithMany()
                    .HasForeignKey(c => c.LanguageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.LanguageId, c.Position });
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.Property(l => l.Title).IsRequired().HasMaxLength(150);
                entity.Property(l => l.Topic).HasMaxLength(150);

                entity.HasOne(l => l.Course)
                    .WithMany(c => c.Lessons)
                    .HasForeignKey(l => l.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(l => new { l.CourseId, l.Position });
            });

            modelBuilder.Entity<LessonWord>(entity =>
            {
                entity.HasOne(lw => lw.Lesson)
                    .WithMany(l => l.LessonWords)
                    .HasForeignKey(lw => lw.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(lw => lw.Word)
                    .WithMany()
                    .HasForeignKey(lw => lw.WordId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(lw => new { lw.LessonId, lw.WordId }).IsUnique();
            });

            modelBuilder.Entity<StudySession>(entity =>
            {
                entity.HasOne(ss => ss.User)
                    .WithMany()
                    .HasForeignKey(ss => ss.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ss => ss.Language)
                    .WithMany()
                    .HasForeignKey(ss => ss.LanguageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ss => ss.Lesson)
                    .WithMany(l => l.StudySessions)
                    .HasForeignKey(ss => ss.LessonId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(ss => new { ss.UserId, ss.StartedAt });
            });

            modelBuilder.Entity<ExerciseAttempt>(entity =>
            {
                entity.Property(ea => ea.GivenAnswer).HasMaxLength(500);
                entity.Property(ea => ea.ExpectedAnswer).HasMaxLength(500);

                entity.HasOne(ea => ea.StudySession)
                    .WithMany(ss => ss.ExerciseAttempts)
                    .HasForeignKey(ea => ea.StudySessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ea => ea.Word)
                    .WithMany()
                    .HasForeignKey(ea => ea.WordId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ea => ea.Sentence)
                    .WithMany()
                    .HasForeignKey(ea => ea.SentenceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(ea => new { ea.WordId, ea.AnsweredAt });
            });

            modelBuilder.Entity<WordPerformance>(entity =>
            {
                entity.HasOne(wp => wp.User)
                    .WithMany()
                    .HasForeignKey(wp => wp.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wp => wp.Word)
                    .WithMany()
                    .HasForeignKey(wp => wp.WordId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(wp => new { wp.UserId, wp.WordId }).IsUnique();
                entity.HasIndex(wp => new { wp.UserId, wp.NextReview });
                entity.HasIndex(wp => new { wp.UserId, wp.LastReview });
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
