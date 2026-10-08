using MemoLingo.Domain.Entities;

namespace MemoLingo.Application.Services
{
    public interface IWordPerformanceService
    {
        Task<WordPerformance> ApplyAttemptAsync(int userId, int wordId, bool isCorrect, DateTime answeredAt);
        Task<bool> SaveChangesAsync();
    }
}
