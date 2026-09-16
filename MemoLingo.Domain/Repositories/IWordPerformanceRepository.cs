using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de acesso ao desempenho do usuário em cada palavra, base da repetição espaçada.
    /// </summary>
    public interface IWordPerformanceRepository
    {
        Task<IEnumerable<WordPerformance>> GetByUserAsync(int userId);
        Task<WordPerformance> GetByUserAndWordAsync(int userId, int wordId);
        Task AddAsync(WordPerformance performance);
        void Update(WordPerformance performance);
        Task<bool> SaveChangesAsync();
    }
}
