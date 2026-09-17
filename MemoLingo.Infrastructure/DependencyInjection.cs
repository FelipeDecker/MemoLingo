using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using MemoLingo.Infrastructure.Data.Seeding;
using MemoLingo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MemoLingo.Infrastructure
{
    /// <summary>
    /// Registra os serviços de persistência da camada de infraestrutura.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<IWordRepository, WordRepository>();
            services.AddScoped<IWordPerformanceRepository, WordPerformanceRepository>();
            services.AddScoped<IStudySessionRepository, StudySessionRepository>();
            services.AddScoped<IExerciseAttemptRepository, ExerciseAttemptRepository>();

            services.AddScoped<DatabaseSeeder>();

            return services;
        }
    }
}
