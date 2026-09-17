using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Projections;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de acesso ao histórico de tentativas de exercício, sempre recortado
    /// nas tentativas mais recentes de cada palavra.
    /// </summary>
    public interface IExerciseAttemptRepository
    {
        Task<IEnumerable<WordAttemptSample>> GetRecentByLanguageAsync(int userId, int languageId, int sampleSize);
        Task<IEnumerable<WordAttemptSample>> GetRecentByWordAsync(int userId, int wordId, int sampleSize);
        Task AddAsync(ExerciseAttempt attempt);
        Task<bool> SaveChangesAsync();
    }
}
