using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Projections;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class ExerciseAttemptRepository : IExerciseAttemptRepository
    {
        private readonly AppDbContext _context;

        public ExerciseAttemptRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WordAttemptSample>> GetRecentByLanguageAsync(int userId, int languageId, int sampleSize)
        {
            if (sampleSize <= 0)
            {
                return new List<WordAttemptSample>();
            }

            // Para cada palavra do idioma, a consulta traz apenas as N tentativas mais
            // recentes (subconsulta correlacionada -> LATERAL JOIN no PostgreSQL),
            // evitando carregar o histórico completo para a memória.
            return await _context.Words
                .AsNoTracking()
                .Where(w => w.LanguageId == languageId)
                .SelectMany(w => _context.ExerciseAttempts
                    .Where(ea => ea.WordId == w.Id && ea.StudySession.UserId == userId)
                    .OrderByDescending(ea => ea.AnsweredAt)
                    .ThenByDescending(ea => ea.Id)
                    .Take(sampleSize)
                    .Select(ea => new WordAttemptSample
                    {
                        WordId = w.Id,
                        IsCorrect = ea.IsCorrect,
                        AnsweredAt = ea.AnsweredAt
                    }))
                .ToListAsync();
        }

        public async Task<IEnumerable<WordAttemptSample>> GetRecentByWordAsync(int userId, int wordId, int sampleSize)
        {
            if (sampleSize <= 0)
            {
                return new List<WordAttemptSample>();
            }

            return await _context.ExerciseAttempts
                .AsNoTracking()
                .Where(ea => ea.WordId == wordId && ea.StudySession.UserId == userId)
                .OrderByDescending(ea => ea.AnsweredAt)
                .ThenByDescending(ea => ea.Id)
                .Take(sampleSize)
                .Select(ea => new WordAttemptSample
                {
                    WordId = wordId,
                    IsCorrect = ea.IsCorrect,
                    AnsweredAt = ea.AnsweredAt
                })
                .ToListAsync();
        }

        public async Task AddAsync(ExerciseAttempt attempt)
        {
            await _context.ExerciseAttempts.AddAsync(attempt);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
