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

                entity.HasData(
                    new Language { Id = 1, Name = "Inglês", Code = "en" },
                    new Language { Id = 2, Name = "Português", Code = "pt" },
                    new Language { Id = 3, Name = "Espanhol", Code = "es" },
                    new Language { Id = 4, Name = "Italiano", Code = "it" }
                );
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

            base.OnModelCreating(modelBuilder);
        }
    }
}
