using System.Text.RegularExpressions;
using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Projections;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class NuanceService : INuanceService
    {
        private const string BlankToken = "{{blank}}";
        private const char AcceptedAnswersSeparator = '|';
        private const int MaxStrengthLevel = 5;
        private const int MaxAnswerLength = 500;

        // Na primeira passada cada grupo contribui com até 2 exercícios, para a sessão
        // cobrir vários grupos antes de repetir o mesmo contraste.
        private const int MaxExercisesPerGroupFirstPass = 2;

        // Janela de tentativas usada para calcular o score de proficiência do grupo.
        private const int RecentAttemptsSampleSize = 20;

        // Grupos vencidos abaixo desse score entram antes dos grupos nunca vistos.
        private const int PriorityProficiencyThreshold = 80;

        private static readonly int[] ReviewIntervalDays = { 1, 2, 4, 7, 15, 30 };

        private readonly ISynonymGroupRepository _synonymGroupRepository;
        private readonly IUserNuanceProgressRepository _progressRepository;
        private readonly IExerciseAttemptRepository _attemptRepository;
        private readonly IStudySessionRepository _studySessionRepository;
        private readonly IUserRepository _userRepository;

        public NuanceService(
            ISynonymGroupRepository synonymGroupRepository,
            IUserNuanceProgressRepository progressRepository,
            IExerciseAttemptRepository attemptRepository,
            IStudySessionRepository studySessionRepository,
            IUserRepository userRepository)
        {
            _synonymGroupRepository = synonymGroupRepository;
            _progressRepository = progressRepository;
            _attemptRepository = attemptRepository;
            _studySessionRepository = studySessionRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<NuanceExerciseModel>> GetSessionAsync(int userId, int take = 10)
        {
            if (take <= 0)
            {
                return new List<NuanceExerciseModel>();
            }

            var user = await ResolveUserAsync(userId);
            var languageId = user is null ? null : ResolveLanguageId(user);
            if (languageId is null)
            {
                return new List<NuanceExerciseModel>();
            }

            var groups = (await _synonymGroupRepository.GetWithExercisesByLanguageAsync(languageId.Value)).ToList();
            if (groups.Count == 0)
            {
                return new List<NuanceExerciseModel>();
            }

            var progresses = (await _progressRepository.GetByUserAsync(user.Id))
                .ToDictionary(p => p.SynonymGroupId);

            var lastAttemptByExercise = (await _attemptRepository.GetNuanceAttemptsAsync(user.Id, languageId.Value))
                .GroupBy(a => a.NuanceExerciseId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.AnsweredAt).First());

            var now = DateTime.UtcNow;

            // Ordem de prioridade: grupos marcados por erro recente, grupos vencidos com
            // score baixo, grupos inéditos e, por fim, os demais pela próxima revisão.
            var orderedGroups = groups
                .Select(group =>
                {
                    progresses.TryGetValue(group.Id, out var progress);
                    return new
                    {
                        Group = group,
                        Progress = progress,
                        Exercises = new Queue<NuanceExercise>(OrderExercises(group.Exercises, lastAttemptByExercise)),
                        Tiebreak = Random.Shared.Next()
                    };
                })
                .OrderBy(x => GetPriorityBucket(x.Progress, now))
                .ThenBy(x => x.Progress?.ProficiencyScore ?? 0)
                .ThenByDescending(x => x.Progress?.LastErrorAt ?? DateTime.MinValue)
                .ThenBy(x => x.Progress?.NextReview ?? DateTime.MinValue)
                .ThenBy(x => x.Tiebreak)
                .ToList();

            var selected = new List<NuanceExerciseModel>(take);

            foreach (var entry in orderedGroups)
            {
                for (var i = 0; i < MaxExercisesPerGroupFirstPass && entry.Exercises.Count > 0 && selected.Count < take; i++)
                {
                    selected.Add(ToExerciseModel(entry.Exercises.Dequeue(), entry.Group, entry.Progress));
                }
            }

            // Se ainda sobrou espaço (poucos grupos), completa em rodízio entre os grupos.
            while (selected.Count < take && orderedGroups.Any(x => x.Exercises.Count > 0))
            {
                foreach (var entry in orderedGroups.Where(x => x.Exercises.Count > 0))
                {
                    if (selected.Count >= take)
                    {
                        break;
                    }

                    selected.Add(ToExerciseModel(entry.Exercises.Dequeue(), entry.Group, entry.Progress));
                }
            }

            return Shuffle(selected);
        }

        public async Task<NuanceAnswerResultModel> SubmitAnswerAsync(NuanceAnswerModel answer)
        {
            if (answer is null)
            {
                throw new ArgumentException("Resposta não informada.", nameof(answer));
            }

            if (string.IsNullOrWhiteSpace(answer.SubmittedAnswer) && answer.SubmittedWordId is null)
            {
                throw new ArgumentException("Informe a palavra escolhida.", nameof(answer));
            }

            var user = await ResolveUserAsync(answer.UserId)
                ?? throw new ArgumentException("Nenhum usuário disponível para registrar a resposta.", nameof(answer));

            var exercise = await _synonymGroupRepository.GetExerciseWithGroupAsync(answer.ExerciseId)
                ?? throw new ArgumentException("Exercício de nuance não encontrado.", nameof(answer));

            var group = exercise.SynonymGroup;
            var items = group.Items.OrderBy(i => i.Position).ThenBy(i => i.Id).ToList();
            var normalizedAnswer = Normalize(answer.SubmittedAnswer);

            var submittedWordId = items.Any(i => i.WordId == answer.SubmittedWordId)
                ? answer.SubmittedWordId
                : DetectSubmittedWord(group, items, normalizedAnswer);

            // O texto digitado tem prioridade; o id da palavra só decide quando nada foi digitado.
            var isCorrect = normalizedAnswer.Length > 0
                ? GetAcceptedForms(exercise).Contains(normalizedAnswer)
                : submittedWordId == exercise.TargetWordId;

            if (isCorrect)
            {
                submittedWordId = exercise.TargetWordId;
            }

            var correctAnswer = GetCorrectAnswer(exercise);
            var givenAnswer = normalizedAnswer.Length > 0
                ? answer.SubmittedAnswer.Trim()
                : items.First(i => i.WordId == submittedWordId).Word.Text;

            var now = DateTime.UtcNow;

            await RegisterAttemptAsync(user.Id, group.LanguageId, exercise.Id, correctAnswer, givenAnswer, isCorrect, now);

            var progress = await UpdateProgressAsync(user.Id, group.Id, isCorrect, now);

            return new NuanceAnswerResultModel
            {
                ExerciseId = exercise.Id,
                IsCorrect = isCorrect,
                SubmittedAnswer = givenAnswer,
                SubmittedWordId = submittedWordId,
                CorrectWordId = exercise.TargetWordId,
                CorrectWord = exercise.TargetWord.Text,
                CorrectAnswer = correctAnswer,
                CompletedSentence = exercise.SentenceContext.Replace(BlankToken, correctAnswer),
                SentenceTranslation = exercise.SentenceTranslation,
                Explanation = exercise.Explanation,
                Nuances = items
                    .Select(item => new NuanceItemModel
                    {
                        WordId = item.WordId,
                        Text = item.Word.Text,
                        Translation = item.Word.Translation,
                        NuanceExplanation = item.NuanceExplanation,
                        IsCorrectAnswer = item.WordId == exercise.TargetWordId,
                        IsSubmittedAnswer = item.WordId == submittedWordId
                    })
                    .ToList(),
                GroupProgress = ToProgressModel(progress)
            };
        }

        public async Task FlagGroupsForWordErrorAsync(int userId, int wordId)
        {
            var groupIds = (await _synonymGroupRepository.GetGroupIdsByWordAsync(wordId)).ToList();
            if (groupIds.Count == 0)
            {
                return;
            }

            var existing = (await _progressRepository.GetByUserAndGroupsAsync(userId, groupIds))
                .ToDictionary(p => p.SynonymGroupId);

            var now = DateTime.UtcNow;

            foreach (var groupId in groupIds)
            {
                // Errar uma palavra do grupo em qualquer exercício coloca o grupo
                // inteiro no topo da fila da prática de nuances.
                if (existing.TryGetValue(groupId, out var progress))
                {
                    progress.IsFlaggedForReview = true;
                    progress.LastErrorAt = now;
                    progress.NextReview = now;
                    _progressRepository.Update(progress);
                }
                else
                {
                    await _progressRepository.AddAsync(new UserNuanceProgress
                    {
                        UserId = userId,
                        SynonymGroupId = groupId,
                        IsFlaggedForReview = true,
                        LastErrorAt = now,
                        NextReview = now
                    });
                }
            }

            await _progressRepository.SaveChangesAsync();
        }

        private async Task RegisterAttemptAsync(
            int userId,
            int languageId,
            int exerciseId,
            string expectedAnswer,
            string givenAnswer,
            bool isCorrect,
            DateTime answeredAt)
        {
            var session = await _studySessionRepository.GetOrCreatePracticeSessionAsync(userId, languageId);

            // A tentativa referencia o exercício de nuance (e não a palavra), para não
            // interferir no percentual de aprendizado das palavras no dicionário.
            await _attemptRepository.AddAsync(new ExerciseAttempt
            {
                StudySessionId = session.Id,
                NuanceExerciseId = exerciseId,
                ExerciseType = ExerciseType.NuanceDiscrimination,
                ExpectedAnswer = Truncate(expectedAnswer),
                GivenAnswer = Truncate(givenAnswer),
                IsCorrect = isCorrect,
                AnsweredAt = answeredAt
            });

            await _attemptRepository.SaveChangesAsync();
        }

        private async Task<UserNuanceProgress> UpdateProgressAsync(int userId, int groupId, bool isCorrect, DateTime now)
        {
            var progress = (await _progressRepository.GetByUserAndGroupsAsync(userId, new[] { groupId })).FirstOrDefault();
            var isNew = progress is null;

            if (isNew)
            {
                progress = new UserNuanceProgress
                {
                    UserId = userId,
                    SynonymGroupId = groupId
                };
            }

            if (isCorrect)
            {
                progress.CorrectCount++;
                progress.StrengthLevel = Math.Min(progress.StrengthLevel + 1, MaxStrengthLevel);
                progress.IsFlaggedForReview = false;
                progress.NextReview = now.AddDays(ReviewIntervalDays[progress.StrengthLevel]);
            }
            else
            {
                progress.WrongCount++;
                progress.StrengthLevel = Math.Max(progress.StrengthLevel - 1, 0);
                progress.IsFlaggedForReview = true;
                progress.LastErrorAt = now;
                progress.NextReview = now;
            }

            progress.LastReview = now;

            var recent = await _attemptRepository.GetRecentNuanceByGroupAsync(userId, groupId, RecentAttemptsSampleSize);
            progress.ProficiencyScore = CalculateProficiencyScore(recent);

            if (isNew)
            {
                await _progressRepository.AddAsync(progress);
            }
            else
            {
                _progressRepository.Update(progress);
            }

            await _progressRepository.SaveChangesAsync();

            return progress;
        }

        // Ordena os exercícios de um grupo: primeiro os que o usuário errou na última
        // tentativa, depois os inéditos e, por fim, os acertados há mais tempo.
        private static IEnumerable<NuanceExercise> OrderExercises(
            IEnumerable<NuanceExercise> exercises,
            Dictionary<int, NuanceAttemptSample> lastAttemptByExercise)
        {
            return exercises
                .Select(exercise =>
                {
                    lastAttemptByExercise.TryGetValue(exercise.Id, out var last);
                    return new { Exercise = exercise, Last = last, Tiebreak = Random.Shared.Next() };
                })
                .OrderBy(x => x.Last is null ? 1 : x.Last.IsCorrect ? 2 : 0)
                .ThenBy(x => x.Last?.AnsweredAt ?? DateTime.MinValue)
                .ThenBy(x => x.Tiebreak)
                .Select(x => x.Exercise);
        }

        private static int GetPriorityBucket(UserNuanceProgress progress, DateTime now)
        {
            if (progress is null)
            {
                return 2;
            }

            if (progress.IsFlaggedForReview)
            {
                return 0;
            }

            if (progress.NextReview <= now)
            {
                return progress.ProficiencyScore < PriorityProficiencyThreshold ? 1 : 3;
            }

            return 4;
        }

        private static NuanceExerciseModel ToExerciseModel(NuanceExercise exercise, SynonymGroup group, UserNuanceProgress progress)
        {
            var options = group.Items
                .Select(item => new NuanceOptionModel
                {
                    WordId = item.WordId,
                    Text = item.Word.Text,
                    AnswerForm = ToBaseForm(item.Word.Text)
                })
                .ToList();

            return new NuanceExerciseModel
            {
                Id = exercise.Id,
                SynonymGroupId = group.Id,
                GroupName = group.Name,
                GroupMeaning = group.Meaning,
                CefrLevel = group.CefrLevel,
                SentenceContext = exercise.SentenceContext,
                SentenceTranslation = exercise.SentenceTranslation,
                IsPriorityReview = progress?.IsFlaggedForReview ?? false,
                GroupProficiencyScore = progress?.ProficiencyScore ?? 0,
                Options = Shuffle(options)
            };
        }

        private static NuanceGroupProgressModel ToProgressModel(UserNuanceProgress progress)
        {
            return new NuanceGroupProgressModel
            {
                SynonymGroupId = progress.SynonymGroupId,
                ProficiencyScore = progress.ProficiencyScore,
                StrengthLevel = progress.StrengthLevel,
                CorrectCount = progress.CorrectCount,
                WrongCount = progress.WrongCount,
                IsFlaggedForReview = progress.IsFlaggedForReview,
                NextReview = progress.NextReview
            };
        }

        // Formas aceitas para a lacuna: a forma base da palavra correta (a conjugação
        // não é o foco do exercício) e as formas flexionadas cadastradas no exercício.
        private static HashSet<string> GetAcceptedForms(NuanceExercise exercise)
        {
            var forms = SplitAcceptedAnswers(exercise.AcceptedAnswers)
                .Select(Normalize)
                .Where(form => form.Length > 0)
                .ToHashSet();

            forms.Add(Normalize(exercise.TargetWord.Text));

            return forms;
        }

        private static string GetCorrectAnswer(NuanceExercise exercise)
        {
            return SplitAcceptedAnswers(exercise.AcceptedAnswers).FirstOrDefault()
                ?? ToBaseForm(exercise.TargetWord.Text);
        }

        // Descobre qual palavra do grupo o usuário digitou (mesmo flexionada), para que o
        // resultado destaque a nuance da palavra escolhida por engano.
        private static int? DetectSubmittedWord(SynonymGroup group, List<SynonymGroupItem> items, string normalizedAnswer)
        {
            if (normalizedAnswer.Length == 0)
            {
                return null;
            }

            foreach (var item in items)
            {
                var forms = group.Exercises
                    .Where(e => e.TargetWordId == item.WordId)
                    .SelectMany(e => SplitAcceptedAnswers(e.AcceptedAnswers))
                    .Select(Normalize)
                    .Append(Normalize(item.Word.Text));

                if (forms.Contains(normalizedAnswer))
                {
                    return item.WordId;
                }
            }

            return null;
        }

        private static IEnumerable<string> SplitAcceptedAnswers(string acceptedAnswers)
        {
            return string.IsNullOrWhiteSpace(acceptedAnswers)
                ? Enumerable.Empty<string>()
                : acceptedAnswers
                    .Split(AcceptedAnswersSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var text = value
                .Replace('\u2019', '\'')
                .Replace('\u2018', '\'')
                .Trim()
                .Trim('.', ',', '!', '?', ';', ':', '"')
                .ToLowerInvariant();

            text = Regex.Replace(text, @"\s+", " ").Trim();

            return ToBaseForm(text);
        }

        private static string ToBaseForm(string text)
        {
            return text.StartsWith("to ", StringComparison.OrdinalIgnoreCase) ? text[3..] : text;
        }

        private static int CalculateProficiencyScore(IEnumerable<NuanceAttemptSample> samples)
        {
            var list = samples.ToList();
            if (list.Count == 0)
            {
                return 0;
            }

            var accuracy = (int)Math.Round((double)list.Count(s => s.IsCorrect) / list.Count * 100, MidpointRounding.AwayFromZero);

            return Math.Clamp(accuracy, 0, 100);
        }

        private static string Truncate(string value)
        {
            return value is { Length: > MaxAnswerLength } ? value[..MaxAnswerLength] : value;
        }

        private static List<T> Shuffle<T>(List<T> items)
        {
            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = Random.Shared.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }

            return items;
        }

        private async Task<User> ResolveUserAsync(int userId)
        {
            return userId > 0
                ? await _userRepository.GetWithProgressesAsync(userId)
                : await _userRepository.GetDefaultAsync();
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
