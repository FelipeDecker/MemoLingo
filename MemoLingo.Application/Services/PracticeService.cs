using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Projections;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class PracticeService : IPracticeService
    {
        // Percentual a partir do qual a palavra é considerada dominada.
        private const int MasteredPercentage = 80;

        // Janela de tentativas usada no modo Premium de "últimas tentativas".
        private const int RecentAttemptsSampleSize = 100;

        // Tamanho padrão da sessão de Prática Focada (6 difíceis + 2 esquecidas + 2 dominadas).
        private const int FocusedPracticeSize = 10;

        // Frações fixas dos grupos B (esquecidas) e C (dominadas) dentro da sessão.
        private const int ForgottenBucketDivisor = 5;
        private const int MasteredBucketDivisor = 5;

        private readonly IWordRepository _wordRepository;
        private readonly IWordPerformanceRepository _performanceRepository;
        private readonly IWordPerformanceService _performanceService;
        private readonly IUserRepository _userRepository;
        private readonly IStudySessionRepository _studySessionRepository;
        private readonly IExerciseAttemptRepository _attemptRepository;
        private readonly INuanceService _nuanceService;

        public PracticeService(
            IWordRepository wordRepository,
            IWordPerformanceRepository performanceRepository,
            IWordPerformanceService performanceService,
            IUserRepository userRepository,
            IStudySessionRepository studySessionRepository,
            IExerciseAttemptRepository attemptRepository,
            INuanceService nuanceService)
        {
            _wordRepository = wordRepository;
            _performanceRepository = performanceRepository;
            _performanceService = performanceService;
            _userRepository = userRepository;
            _studySessionRepository = studySessionRepository;
            _attemptRepository = attemptRepository;
            _nuanceService = nuanceService;
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

            var statsMode = ResolveStatsMode(user);
            var stats = await LoadStatsAsync(user.Id, languageId.Value, statsMode, performances);

            var models = words
                .Select(word => ToModel(word, performances.GetValueOrDefault(word.Id), stats.GetValueOrDefault(word.Id), statsMode))
                // O diferencial do app: as palavras com mais dificuldade vêm primeiro.
                // Palavras sem nenhuma tentativa ("virgens") ficam no fim da lista.
                .OrderBy(w => w.SampleAttemptCount == 0)
                .ThenBy(w => w.LearningPercentage)
                .ThenByDescending(w => w.SampleWrongCount)
                .ThenByDescending(w => w.WrongCount)
                .ThenBy(w => w.Text)
                .AsEnumerable();

            if (take is > 0)
            {
                models = models.Take(take.Value);
            }

            return models.ToList();
        }

        public async Task<IEnumerable<PracticeWordModel>> GetWordsByLevelAsync(int? userId, int languageId, CefrLevel cefrLevel)
        {
            var user = await ResolveUserAsync(userId);

            var words = (await _wordRepository.GetByLanguageAsync(languageId))
                .Where(w => w.CefrLevel == cefrLevel)
                .ToList();

            var performances = user is null
                ? new Dictionary<int, WordPerformance>()
                : (await _performanceRepository.GetByUserAsync(user.Id)).ToDictionary(wp => wp.WordId);

            var statsMode = user is null ? LearningStatsMode.Total : ResolveStatsMode(user);
            var stats = user is null
                ? new Dictionary<int, WordStats>()
                : await LoadStatsAsync(user.Id, languageId, statsMode, performances);

            return words
                .Select(word => ToModel(word, performances.GetValueOrDefault(word.Id), stats.GetValueOrDefault(word.Id), statsMode))
                .OrderBy(w => w.SampleAttemptCount == 0)
                .ThenBy(w => w.LearningPercentage)
                .ThenByDescending(w => w.SampleWrongCount)
                .ThenBy(w => w.Text)
                .ToList();
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

            var statsMode = ResolveStatsMode(user);
            var stats = await LoadStatsAsync(user.Id, languageId.Value, statsMode, performances);

            var pool = learnedWords
                .Select(word => ToModel(word, performances.GetValueOrDefault(word.Id), stats.GetValueOrDefault(word.Id), statsMode))
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
                .ThenByDescending(w => w.SampleWrongCount)
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

            // GRUPO C: palavras dominadas (percentual de acerto alto).
            AddRange(selected, selectedIds, pool
                .Where(w => !selectedIds.Contains(w.Id))
                .Where(w => w.SampleAttemptCount > 0 && w.LearningPercentage >= MasteredPercentage)
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

        public async Task<IEnumerable<PracticeWordModel>> GetPhrasalVerbsPracticeAsync(int userId, int take = FocusedPracticeSize)
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

            // A coleção de verbos frasais não depende da trilha: todos ficam disponíveis.
            var phrasalVerbs = await _wordRepository.GetByLanguageAndPartOfSpeechAsync(languageId.Value, PartOfSpeech.PhrasalVerb);

            var performances = (await _performanceRepository.GetByUserAsync(user.Id))
                .ToDictionary(wp => wp.WordId);

            var statsMode = ResolveStatsMode(user);
            var stats = await LoadStatsAsync(user.Id, languageId.Value, statsMode, performances);

            // Prioriza os verbos com mais erros; empates são sorteados para variar as sessões.
            var selected = phrasalVerbs
                .Select(word => ToModel(word, performances.GetValueOrDefault(word.Id), stats.GetValueOrDefault(word.Id), statsMode))
                .OrderBy(w => w.LearningPercentage)
                .ThenByDescending(w => w.SampleWrongCount)
                .ThenBy(_ => Random.Shared.Next())
                .Take(take)
                .ToList();

            return Shuffle(selected);
        }

        public async Task<PracticeWordModel> RegisterResultAsync(PracticeResultModel result)
        {
            var user = await ResolveUserAsync(result.UserId > 0 ? result.UserId : null)
                ?? throw new ArgumentException("Nenhum usuário disponível para registrar o resultado.", nameof(result));

            var word = await _wordRepository.GetByIdAsync(result.WordId)
                ?? throw new ArgumentException("Palavra não encontrada.", nameof(result));

            var now = DateTime.UtcNow;
            var performance = await _performanceService.ApplyAttemptAsync(user.Id, word.Id, result.Correct, now);

            await _performanceService.SaveChangesAsync();

            await RegisterAttemptAsync(user, word, result.Correct, now);

            if (!result.Correct)
            {
                // Errar uma palavra que faz parte de um grupo de nuances prioriza esse grupo.
                await _nuanceService.FlagGroupsForWordErrorAsync(user.Id, word.Id);
            }

            return await ToModelAsync(user, word, performance);
        }

        public async Task<PracticeWordModel> RegisterWrongAttemptAsync(PracticeWrongAttemptModel model)
        {
            var user = await ResolveUserAsync(model.UserId > 0 ? model.UserId : null)
                ?? throw new ArgumentException("Nenhum usuário disponível para registrar a tentativa.", nameof(model));

            var word = await _wordRepository.GetByIdAsync(model.WordId)
                ?? throw new ArgumentException("Palavra não encontrada.", nameof(model));

            var now = DateTime.UtcNow;
            var performance = await _performanceService.ApplyAttemptAsync(user.Id, word.Id, false, now);

            // Errar significa que a palavra volta imediatamente para a fila de revisão.
            performance.NextReview = now;

            await _performanceService.SaveChangesAsync();

            // O clique do usuário vira uma tentativa errada no histórico da palavra.
            await RegisterAttemptAsync(user, word, false, now);

            await _nuanceService.FlagGroupsForWordErrorAsync(user.Id, word.Id);

            return await ToModelAsync(user, word, performance);
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

        // O modo "últimas tentativas" só vale para assinantes Premium que o habilitaram no perfil.
        private static LearningStatsMode ResolveStatsMode(User user)
        {
            return user.Plan == SubscriptionPlan.Premium && user.LearningStatsMode == LearningStatsMode.RecentAttempts
                ? LearningStatsMode.RecentAttempts
                : LearningStatsMode.Total;
        }

        private async Task<Dictionary<int, WordStats>> LoadStatsAsync(
            int userId,
            int languageId,
            LearningStatsMode statsMode,
            Dictionary<int, WordPerformance> performances)
        {
            if (statsMode == LearningStatsMode.RecentAttempts)
            {
                var samples = await _attemptRepository.GetRecentByLanguageAsync(userId, languageId, RecentAttemptsSampleSize);

                return samples
                    .GroupBy(sample => sample.WordId)
                    .ToDictionary(group => group.Key, group => WordStats.FromSamples(group));
            }

            return performances.ToDictionary(pair => pair.Key, pair => WordStats.FromPerformance(pair.Value));
        }

        private async Task<PracticeWordModel> ToModelAsync(User user, Word word, WordPerformance performance)
        {
            var statsMode = ResolveStatsMode(user);

            var stats = statsMode == LearningStatsMode.RecentAttempts
                ? WordStats.FromSamples(await _attemptRepository.GetRecentByWordAsync(user.Id, word.Id, RecentAttemptsSampleSize))
                : WordStats.FromPerformance(performance);

            return ToModel(word, performance, stats, statsMode);
        }

        private static PracticeWordModel ToModel(Word word, WordPerformance performance, WordStats stats, LearningStatsMode statsMode)
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
                StatsMode = statsMode,
                SampleAttemptCount = stats?.Total ?? 0,
                SampleCorrectCount = stats?.Correct ?? 0,
                SampleWrongCount = stats?.Wrong ?? 0,
                LearningPercentage = CalculateLearningPercentage(stats),
                LastReview = performance?.LastReview,
                NextReview = performance?.NextReview
            };
        }

        // (acertos / tentativas consideradas) * 100, seja no total acumulado ou na janela recente.
        private static int CalculateLearningPercentage(WordStats stats)
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

        private sealed class WordStats
        {
            public int Total { get; init; }
            public int Correct { get; init; }
            public int Wrong => Total - Correct;

            public static WordStats FromPerformance(WordPerformance performance)
            {
                if (performance is null)
                {
                    return null;
                }

                return new WordStats
                {
                    Total = performance.CorrectCount + performance.WrongCount,
                    Correct = performance.CorrectCount
                };
            }

            public static WordStats FromSamples(IEnumerable<WordAttemptSample> samples)
            {
                var list = samples.ToList();

                return new WordStats
                {
                    Total = list.Count,
                    Correct = list.Count(sample => sample.IsCorrect)
                };
            }
        }
    }
}
