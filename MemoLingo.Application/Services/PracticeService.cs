using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class PracticeService : IPracticeService
    {
        private const int MaxStrengthLevel = 5;

        private static readonly int[] ReviewIntervalDays = { 1, 2, 4, 7, 15, 30 };

        private readonly IWordRepository _wordRepository;
        private readonly IWordPerformanceRepository _performanceRepository;
        private readonly IUserRepository _userRepository;

        public PracticeService(
            IWordRepository wordRepository,
            IWordPerformanceRepository performanceRepository,
            IUserRepository userRepository)
        {
            _wordRepository = wordRepository;
            _performanceRepository = performanceRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<PracticeWordModel>> GetWordsAsync(int? userId, int? take)
        {
            var user = await ResolveUserAsync(userId);
            if (user is null)
            {
                return new List<PracticeWordModel>();
            }

            var languageId = ResolveLanguageId(user);
            if (languageId is null)
            {
                return new List<PracticeWordModel>();
            }

            var words = await _wordRepository.GetByLanguageAsync(languageId.Value);
            var performances = (await _performanceRepository.GetByUserAsync(user.Id))
                .ToDictionary(wp => wp.WordId);

            var models = words
                .Select(word =>
                {
                    performances.TryGetValue(word.Id, out var performance);
                    return ToModel(word, performance);
                })
                // O diferencial do app: as palavras em que o usuário mais erra vêm primeiro.
                .OrderByDescending(w => w.WrongCount)
                .ThenBy(w => GetAccuracyRate(w))
                .ThenBy(w => w.Text)
                .AsEnumerable();

            if (take is > 0)
            {
                models = models.Take(take.Value);
            }

            return models.ToList();
        }

        public async Task<PracticeWordModel> RegisterResultAsync(PracticeResultModel result)
        {
            var user = await ResolveUserAsync(result.UserId > 0 ? result.UserId : null)
                ?? throw new ArgumentException("Nenhum usuário disponível para registrar o resultado.", nameof(result));

            var word = await _wordRepository.GetByIdAsync(result.WordId)
                ?? throw new ArgumentException("Palavra não encontrada.", nameof(result));

            var performance = await _performanceRepository.GetByUserAndWordAsync(user.Id, word.Id);
            var isNew = performance is null;

            if (isNew)
            {
                performance = new WordPerformance
                {
                    UserId = user.Id,
                    WordId = word.Id
                };
            }

            if (result.Correct)
            {
                performance.CorrectCount++;
                performance.StrengthLevel = Math.Min(performance.StrengthLevel + 1, MaxStrengthLevel);
            }
            else
            {
                performance.WrongCount++;
                performance.StrengthLevel = Math.Max(performance.StrengthLevel - 1, 0);
            }

            var now = DateTime.UtcNow;
            performance.LastReview = now;
            performance.NextReview = now.AddDays(ReviewIntervalDays[performance.StrengthLevel]);

            if (isNew)
            {
                await _performanceRepository.AddAsync(performance);
            }
            else
            {
                _performanceRepository.Update(performance);
            }

            await _performanceRepository.SaveChangesAsync();

            return ToModel(word, performance);
        }

        private async Task<User> ResolveUserAsync(int? userId)
        {
            return userId.HasValue
                ? await _userRepository.GetWithProgressesAsync(userId.Value)
                : await _userRepository.GetDefaultAsync();
        }

        private static PracticeWordModel ToModel(Word word, WordPerformance performance)
        {
            return new PracticeWordModel
            {
                Id = word.Id,
                LanguageId = word.LanguageId,
                Text = word.Text,
                Translation = word.Translation,
                CefrLevel = word.CefrLevel,
                PartOfSpeech = word.PartOfSpeech,
                CorrectCount = performance?.CorrectCount ?? 0,
                WrongCount = performance?.WrongCount ?? 0,
                StrengthLevel = performance?.StrengthLevel ?? 0,
                LastReview = performance?.LastReview,
                NextReview = performance?.NextReview
            };
        }

        private static double GetAccuracyRate(PracticeWordModel word)
        {
            var total = word.CorrectCount + word.WrongCount;
            return total == 0 ? 0 : (double)word.CorrectCount / total;
        }

        private static int? ResolveLanguageId(User user)
        {
            if (user.LanguageProgresses is null || user.LanguageProgresses.Count == 0)
            {
                return null;
            }

            var active = user.LanguageProgresses.FirstOrDefault(lp => lp.IsActiveCourse)
                ?? user.LanguageProgresses.First();

            return active.LanguageId;
        }
    }
}
