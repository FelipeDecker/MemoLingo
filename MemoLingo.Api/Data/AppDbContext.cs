using MemoLingo.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();

        public DbSet<Idioma> Idiomas => Set<Idioma>();

        public DbSet<UsuarioIdioma> UsuarioIdiomas => Set<UsuarioIdioma>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Nome).IsRequired().HasMaxLength(150);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);

                entity.HasOne(u => u.IdiomaMaterno)
                    .WithMany()
                    .HasForeignKey(u => u.IdiomaMaternoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Idioma>(entity =>
            {
                entity.Property(i => i.Nome).IsRequired().HasMaxLength(50);
                entity.Property(i => i.Codigo).IsRequired().HasMaxLength(10);
                entity.HasIndex(i => i.Codigo).IsUnique();

                entity.HasData(
                    new Idioma { Id = 1, Nome = "Inglês", Codigo = "en" },
                    new Idioma { Id = 2, Nome = "Português", Codigo = "pt" },
                    new Idioma { Id = 3, Nome = "Espanhol", Codigo = "es" },
                    new Idioma { Id = 4, Nome = "Italiano", Codigo = "it" }
                );
            });

            modelBuilder.Entity<UsuarioIdioma>(entity =>
            {
                entity.HasOne(ui => ui.Usuario)
                    .WithMany(u => u.UsuarioIdiomas)
                    .HasForeignKey(ui => ui.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ui => ui.Idioma)
                    .WithMany()
                    .HasForeignKey(ui => ui.IdiomaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(ui => new { ui.UsuarioId, ui.IdiomaId }).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
