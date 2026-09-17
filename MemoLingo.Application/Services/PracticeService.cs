using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Projections;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class PracticeService : IPracticeService
    {
        private const int MaxStrengthLevel = 5;

        // Janela de tentativas usada para calcular o percentual de aprendizado.
        private const int RecentAttemptsSampleSize = 100;

        // Tamanho padrão da sessão de Prática Focada (6 difíceis + 2 esquecidas + 2 dominadas).
        private const int FocusedPracticeSize = 10;

        // Frações fixas dos grupos B (esquecidas) e C (dominadas) dentro da sessão.
        private const int ForgottenBucketDivisor = 5;
        private const int MasteredBucketDivisor = 5;

        private static readonly int[] ReviewIntervalDays = { 1, 2, 4, 7, 15, 30 };

        private readonly IWordRepository _wordRepository;
        private readonly IWordPerformanceRepository _performanceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStudySessionRepository _studySessionRepository;
        private readonly IExerciseAttemptRepository _attemptRepository;

        public PracticeService(
            IWordRepository wordRepository,
            IWordPerformanceRepository performanceRepository,
            IUserRepository userRepository,
            IStudySessionRepository studySessionRepository,
            IExerciseAttemptRepository attemptRepository)
        {
            _wordRepository = wordRepository;
            _performanceRepository = performanceRepository;
            _userRepository = userRepository;
            _studySessionRepository = studySessionRepository;
            _attemptRepository = attemptRepository;
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

            // O histórico já vem recortado nas últimas tentativas de cada palavra.
            var recentStats = BuildRecentStats(
                await _attemptRepository.GetRecentByLanguageAsync(user.Id, languageId.Value, RecentAttemptsSampleSize));

            var models = words
                .Select(word =>
                {
                    performances.TryGetValue(word.Id, out var performance);
                    recentStats.TryGetValue(word.Id, out var stats);
                    return ToModel(word, performance, stats);
                })
                // O diferencial do app: as palavras com mais dificuldade vêm primeiro.
                // Palavras sem nenhuma tentativa ("virgens") ficam no fim da lista.
                .OrderBy(w => w.RecentAttemptCount == 0)
                .ThenBy(w => w.LearningPercentage)
                .ThenByDescending(w => w.RecentWrongCount)
                .ThenByDescending(w => w.WrongCount)
                .ThenBy(w => w.Text)
                .AsEnumerable();

            if (take is > 0)
            {
                models = models.Take(take.Value);
            }

            return models.ToList();
        }

        public async Task<IEnumerable<PracticeWordModel>> GetFocusedPracticeWordsAsync(int userId, int take = FocusedPracticeSize)
        {
            if (take <= 0)
            {
                return new List<PracticeWordModel>();
            }

            var user = await ResolveUserAsync(userId > 0 ? userId : null);
            if (user is null)
            {
                return new List<PracticeWordModel>();
            }

            var languageId = ResolveLanguageId(user);
            if (languageId is null)
            {
                return new List<PracticeWordModel>();
            }

            // O pool contém apenas palavras já aprendidas: praticadas alguma vez
            // ou vistas em lições de nós da trilha já concluídos.
            var learnedWords = await _wordRepository.GetLearnedByUserAsync(user.Id, languageId.Value);

            var performances = (await _performanceRepository.GetByUserAsync(user.Id))
                .ToDictionary(wp => wp.WordId);

            var recentStats = BuildRecentStats(
                await _attemptRepository.GetRecentByLanguageAsync(user.Id, languageId.Value, RecentAttemptsSampleSize));

            var pool = learnedWords
                .Select(word =>
                {
                    performances.TryGetValue(word.Id, out var performance);
                    recentStats.TryGetValue(word.Id, out var stats);
                    return ToModel(word, performance, stats);
                })
                .ToList();

            if (pool.Count == 0)
            {
                return new List<PracticeWordModel>();
            }

            var forgottenSize = take / ForgottenBucketDivisor;
            var masteredSize = take / MasteredBucketDivisor;
            var hardestSize = take - forgottenSize - masteredSize;

            // Fila mestre de dificuldade: menor percentual de acerto primeiro.
            // Serve tanto para o Grupo A quanto para o fallback das vagas restantes.
            var hardestQueue = pool
                .OrderBy(w => w.LearningPercentage)
                .ThenByDescending(w => w.RecentWrongCount)
                .ThenByDescending(w => w.WrongCount)
                .ThenBy(w => w.Text)
                .ToList();

            var selected = new List<PracticeWordModel>(take);
            var selectedIds = new HashSet<int>();

            // GRUPO A: as palavras com o menor percentual de acerto.
            AddRange(selected, selectedIds, hardestQueue.Take(hardestSize));

            // GRUPO B: as esquecidas há mais tempo (nunca revisadas entram primeiro).
            AddRange(selected, selectedIds, pool
                .Where(w => !selectedIds.Contains(w.Id))
                .OrderBy(w => w.LastReview ?? DateTime.MinValue)
                .ThenBy(w => w.Text)
                .Take(forgottenSize));

            // GRUPO C: palavras dominadas (sem erros na janela recente).
            AddRange(selected, selectedIds, pool
                .Where(w => !selectedIds.Contains(w.Id))
                .Where(w => w.RecentAttemptCount > 0 && w.RecentWrongCount == 0)
                .OrderByDescending(w => w.LearningPercentage)
                .ThenByDescending(w => w.StrengthLevel)
                .ThenBy(w => w.Text)
                .Take(masteredSize));

            // Fallback: se faltou palavra nos grupos B ou C, completa as vagas
            // com as próximas palavras mais difíceis.
            if (selected.Count < take)
            {
                AddRange(selected, selectedIds, hardestQueue
                    .Where(w => !selectedIds.Contains(w.Id))
                    .Take(take - selected.Count));
            }

            return Shuffle(selected);
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

            await RegisterAttemptAsync(user, word, result.Correct, now);

            var stats = await GetRecentStatsAsync(user.Id, word.Id);

            return ToModel(word, performance, stats);
        }

        public async Task<PracticeWordModel> RegisterWrongAttemptAsync(PracticeWrongAttemptModel model)
        {
            var user = await ResolveUserAsync(model.UserId > 0 ? model.UserId : null)
                ?? throw new ArgumentException("Nenhum usuário disponível para registrar a tentativa.", nameof(model));

            var word = await _wordRepository.GetByIdAsync(model.WordId)
                ?? throw new ArgumentException("Palavra não encontrada.", nameof(model));

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

            var now = DateTime.UtcNow;

            performance.WrongCount++;
            performance.StrengthLevel = Math.Max(performance.StrengthLevel - 1, 0);
            performance.LastReview = now;
            // Errar significa que a palavra volta imediatamente para a fila de revisão.
            performance.NextReview = now;

            if (isNew)
            {
                await _performanceRepository.AddAsync(performance);
            }
            else
            {
                _performanceRepository.Update(performance);
            }

            await _performanceRepository.SaveChangesAsync();

            // O clique do usuário vira uma tentativa errada no histórico da palavra.
            await RegisterAttemptAsync(user, word, false, now);

            // Recalcula o percentual considerando a janela das tentativas mais recentes.
            var stats = await GetRecentStatsAsync(user.Id, word.Id);

            return ToModel(word, performance, stats);
        }
        private async Task RegisterAttemptAsync(User user, Word word, bool correct, DateTime answeredAt)
        {
            var session = await _studySessionRepository.GetOrCreatePracticeSessionAsync(user.Id, word.LanguageId);

            await _attemptRepository.AddAsync(new ExerciseAttempt
            {
                StudySessionId = session.Id,
                WordId = word.Id,
                ExerciseType = ExerciseType.FreeTranslation,
                ExpectedAnswer = word.Translation,
                GivenAnswer = correct ? word.Translation : null,
                IsCorrect = correct,
                AnsweredAt = answeredAt
            });

            await _attemptRepository.SaveChangesAsync();
        }

        private async Task<RecentWordStats> GetRecentStatsAsync(int userId, int wordId)
        {
            var samples = await _attemptRepository.GetRecentByWordAsync(userId, wordId, RecentAttemptsSampleSize);
            return RecentWordStats.FromSamples(samples);
        }

        private static Dictionary<int, RecentWordStats> BuildRecentStats(IEnumerable<WordAttemptSample> samples)
        {
            return samples
                .GroupBy(sample => sample.WordId)
                .ToDictionary(group => group.Key, group => RecentWordStats.FromSamples(group));
        }

        private static void AddRange(List<PracticeWordModel> selected, HashSet<int> selectedIds, IEnumerable<PracticeWordModel> candidates)
        {
            foreach (var candidate in candidates)
            {
                if (selectedIds.Add(candidate.Id))
                {
                    selected.Add(candidate);
                }
            }
        }

        /// <summary>
        /// Embaralha a lista final para que o usuário não consiga prever a ordem
        /// de dificuldade durante o exercício.
        /// </summary>
        private static List<PracticeWordModel> Shuffle(List<PracticeWordModel> words)
        {
            for (var i = words.Count - 1; i > 0; i--)
            {
                var j = Random.Shared.Next(i + 1);
                (words[i], words[j]) = (words[j], words[i]);
            }

            return words;
        }

        private async Task<User> ResolveUserAsync(int? userId)
        {
            return userId.HasValue
                ? await _userRepository.GetWithProgressesAsync(userId.Value)
                : await _userRepository.GetDefaultAsync();
        }

        private static PracticeWordModel ToModel(Word word, WordPerformance performance, RecentWordStats stats)
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
                RecentAttemptCount = stats?.Total ?? 0,
                RecentCorrectCount = stats?.Correct ?? 0,
                RecentWrongCount = stats?.Wrong ?? 0,
                LearningPercentage = CalculateLearningPercentage(stats),
                LastReview = performance?.LastReview,
                NextReview = performance?.NextReview
            };
        }

        /// <summary>
        /// O percentual considera apenas a janela das tentativas mais recentes da palavra:
        /// (acertos recentes / total de tentativas recentes) * 100.
        /// </summary>
        private static int CalculateLearningPercentage(RecentWordStats stats)
        {
            if (stats is null || stats.Total == 0)
            {
                return 0;
            }

            var accuracy = (int)Math.Round((double)stats.Correct / stats.Total * 100, MidpointRounding.AwayFromZero);

            return Math.Clamp(accuracy, 0, 100);
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

        private sealed class RecentWordStats
        {
            public int Total { get; init; }
            public int Correct { get; init; }
            public int Wrong => Total - Correct;

            public static RecentWordStats FromSamples(IEnumerable<WordAttemptSample> samples)
            {
                var list = samples.ToList();

                return new RecentWordStats
                {
                    Total = list.Count,
                    Correct = list.Count(sample => sample.IsCorrect)
                };
            }
        }
    }
}
