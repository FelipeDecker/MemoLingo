using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class WordPerformanceService : IWordPerformanceService
    {
        private const int MaxStrengthLevel = 5;

        private static readonly int[] ReviewIntervalDays = { 1, 2, 4, 7, 15, 30 };

        private readonly IWordPerformanceRepository _performanceRepository;

        public WordPerformanceService(IWordPerformanceRepository performanceRepository)
        {
            _performanceRepository = performanceRepository;
        }

        // Apenas rastreia a alteração; quem chama decide quando persistir (SaveChangesAsync).
        public async Task<WordPerformance> ApplyAttemptAsync(int userId, int wordId, bool isCorrect, DateTime answeredAt)
        {
            var performance = await _performanceRepository.GetByUserAndWordAsync(userId, wordId);
            var isNew = performance is null;

            if (isNew)
            {
                performance = new WordPerformance
                {
                    UserId = userId,
                    WordId = wordId
                };
            }

            if (isCorrect)
            {
                performance.CorrectCount++;
                performance.StrengthLevel = Math.Min(performance.StrengthLevel + 1, MaxStrengthLevel);
            }
            else
            {
                performance.WrongCount++;
                performance.StrengthLevel = Math.Max(performance.StrengthLevel - 1, 0);
            }

            performance.LastReview = answeredAt;
            performance.NextReview = answeredAt.AddDays(ReviewIntervalDays[performance.StrengthLevel]);

            if (isNew)
            {
                await _performanceRepository.AddAsync(performance);
            }
            else
            {
                _performanceRepository.Update(performance);
            }

            return performance;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _performanceRepository.SaveChangesAsync();
        }
    }
}
