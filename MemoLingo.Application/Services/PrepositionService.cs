using System.Text.RegularExpressions;
using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Projections;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class PrepositionService : IPrepositionService
    {
        private const string BlankToken = "{{blank}}";
        private const char SlotSeparator = '|';
        private const char AlternativeSeparator = '/';
        private const int MaxSessionSize = 50;
        private const int MaxAcceptedCombinations = 32;

        private static readonly HashSet<string> TrackedPrepositions = new(StringComparer.OrdinalIgnoreCase) { "in", "on", "at" };

        private readonly IPrepositionExerciseRepository _exerciseRepository;
        private readonly IExerciseAttemptRepository _attemptRepository;
        private readonly IStudySessionRepository _studySessionRepository;
        private readonly IUserRepository _userRepository;

        public PrepositionService(
            IPrepositionExerciseRepository exerciseRepository,
            IExerciseAttemptRepository attemptRepository,
            IStudySessionRepository studySessionRepository,
            IUserRepository userRepository)
        {
            _exerciseRepository = exerciseRepository;
            _attemptRepository = attemptRepository;
            _studySessionRepository = studySessionRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<PrepositionExerciseModel>> GetSessionAsync(int userId, int take, PrepositionExerciseType? exerciseType)
        {
            if (take <= 0)
            {
                return new List<PrepositionExerciseModel>();
            }

            take = Math.Min(take, MaxSessionSize);

            var user = await ResolveUserAsync(userId);
            var languageId = user is null ? null : ResolveLanguageId(user);
            if (languageId is null)
            {
                return new List<PrepositionExerciseModel>();
            }

            var exercises = (await _exerciseRepository.GetByLanguageAsync(languageId.Value, exerciseType)).ToList();
            if (exercises.Count == 0)
            {
                return new List<PrepositionExerciseModel>();
            }

            var lastAttemptByExercise = (await _attemptRepository.GetPrepositionAttemptsAsync(user.Id, languageId.Value))
                .GroupBy(a => a.PrepositionExerciseId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.AnsweredAt).First());

            // Prioridade: exercícios errados na última tentativa, depois os inéditos e,
            // por fim, os acertados há mais tempo.
            var selected = exercises
                .Select(exercise =>
                {
                    lastAttemptByExercise.TryGetValue(exercise.Id, out var last);
                    return new { Exercise = exercise, Last = last, Tiebreak = Random.Shared.Next() };
                })
                .OrderBy(x => GetPriorityBucket(x.Last))
                .ThenBy(x => x.Last?.AnsweredAt ?? DateTime.MinValue)
                .ThenBy(x => x.Tiebreak)
                .Take(take)
                .Select(x => ToExerciseModel(x.Exercise, x.Last))
                .ToList();

            return SentenceExerciseHelper.Shuffle(selected);
        }

        public async Task<PrepositionAnswerResultModel> SubmitAnswerAsync(PrepositionAnswerModel answer)
        {
            if (answer is null)
            {
                throw new ArgumentException("Resposta não informada.");
            }

            var user = await ResolveUserAsync(answer.UserId)
                ?? throw new ArgumentException("Nenhum usuário disponível para registrar a resposta.");

            var exercise = await _exerciseRepository.GetByIdAsync(answer.ExerciseId)
                ?? throw new ArgumentException("Exercício de preposição não encontrado.");

            var slots = SplitSlots(exercise.Answers);
            var correctAnswers = slots.Select(s => s[0]).ToList();
            var correctSentence = Fill(exercise.Sentence, correctAnswers);

            var result = new PrepositionAnswerResultModel
            {
                ExerciseId = exercise.Id,
                Sentence = exercise.Sentence,
                CorrectSentence = correctSentence,
                Translation = exercise.Translation,
                Explanation = exercise.Explanation,
                CorrectAnswers = correctAnswers
            };

            string givenAnswer;

            switch (exercise.ExerciseType)
            {
                case PrepositionExerciseType.FillInTheBlanks:
                    givenAnswer = EvaluateBlanks(answer, slots, result);
                    break;

                case PrepositionExerciseType.WriteSentence:
                    givenAnswer = EvaluateSentence(answer, exercise, slots, user, result);
                    break;

                case PrepositionExerciseType.FindTheMistake:
                    givenAnswer = EvaluateMistake(answer, exercise, slots, result);
                    break;

                default:
                    throw new ArgumentException("Tipo de exercício de preposição não suportado.");
            }

            await RegisterAttemptAsync(user.Id, exercise, correctSentence, givenAnswer, result.IsCorrect);

            return result;
        }

        private static string EvaluateBlanks(PrepositionAnswerModel answer, List<List<string>> slots, PrepositionAnswerResultModel result)
        {
            var submitted = answer.Answers ?? new List<string>();

            if (submitted.All(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Preencha as lacunas da frase.");
            }

            result.Blanks = slots
                .Select((alternatives, index) =>
                {
                    var given = index < submitted.Count ? submitted[index]?.Trim() ?? string.Empty : string.Empty;

                    return new PrepositionBlankResultModel
                    {
                        Index = index,
                        SubmittedAnswer = given,
                        CorrectAnswer = alternatives[0],
                        IsCorrect = alternatives.Contains(NormalizeWord(given), StringComparer.OrdinalIgnoreCase)
                    };
                })
                .ToList();

            result.IsCorrect = result.Blanks.All(b => b.IsCorrect);
            result.PrepositionsCorrect = result.IsCorrect;

            return string.Join(" | ", result.Blanks.Select(b => b.SubmittedAnswer));
        }

        private static string EvaluateSentence(
            PrepositionAnswerModel answer,
            PrepositionExercise exercise,
            List<List<string>> slots,
            User user,
            PrepositionAnswerResultModel result)
        {
            if (string.IsNullOrWhiteSpace(answer.SubmittedSentence))
            {
                throw new ArgumentException("Escreva a frase em inglês.");
            }

            var submitted = answer.SubmittedSentence.Trim();
            var accepted = BuildAcceptedSentences(exercise, slots);

            result.SubmittedSentence = submitted;
            result.IsCorrect = SentenceExerciseHelper.IsCorrect(submitted, accepted, SentenceExerciseHelper.AcceptsMissingApostrophes(user));

            // Mesmo quando a frase não bate por completo, informa se as preposições (o foco da
            // coleção) foram usadas na ordem certa.
            var submittedPrepositions = ExtractPrepositions(submitted);
            result.PrepositionsCorrect = result.IsCorrect
                || accepted.Any(sentence => ExtractPrepositions(sentence).SequenceEqual(submittedPrepositions));

            return submitted;
        }

        private static string EvaluateMistake(
            PrepositionAnswerModel answer,
            PrepositionExercise exercise,
            List<List<string>> slots,
            PrepositionAnswerResultModel result)
        {
            var shown = SplitShown(exercise.ShownPrepositions);

            if (answer.MistakeIndex is null || answer.MistakeIndex < 0 || answer.MistakeIndex >= slots.Count)
            {
                throw new ArgumentException("Selecione a preposição que está errada.");
            }

            if (string.IsNullOrWhiteSpace(answer.Correction))
            {
                throw new ArgumentException("Informe a preposição correta.");
            }

            var selectedIndex = answer.MistakeIndex.Value;
            var correction = NormalizeWord(answer.Correction);
            var mistakeIndex = Enumerable.Range(0, slots.Count)
                .FirstOrDefault(i => i < shown.Count && !slots[i].Contains(shown[i], StringComparer.OrdinalIgnoreCase), -1);

            result.MistakeIndex = mistakeIndex < 0 ? null : mistakeIndex;
            result.SubmittedMistakeIndex = selectedIndex;
            result.IsCorrect = selectedIndex == mistakeIndex
                && slots[selectedIndex].Contains(correction, StringComparer.OrdinalIgnoreCase);
            result.PrepositionsCorrect = result.IsCorrect;
            result.Blanks = slots
                .Select((alternatives, index) =>
                {
                    var given = index == selectedIndex ? correction : index < shown.Count ? shown[index] : string.Empty;

                    return new PrepositionBlankResultModel
                    {
                        Index = index,
                        SubmittedAnswer = given,
                        CorrectAnswer = alternatives[0],
                        IsCorrect = alternatives.Contains(given, StringComparer.OrdinalIgnoreCase)
                    };
                })
                .ToList();

            var submittedSentence = Fill(exercise.Sentence, result.Blanks.Select(b => b.SubmittedAnswer).ToList());
            result.SubmittedSentence = submittedSentence;

            return submittedSentence;
        }

        private async Task RegisterAttemptAsync(int userId, PrepositionExercise exercise, string expectedAnswer, string givenAnswer, bool isCorrect)
        {
            var session = await _studySessionRepository.GetOrCreatePracticeSessionAsync(userId, exercise.LanguageId);

            await _attemptRepository.AddAsync(new ExerciseAttempt
            {
                StudySessionId = session.Id,
                PrepositionExerciseId = exercise.Id,
                ExerciseType = ExerciseType.PrepositionPractice,
                ExpectedAnswer = SentenceExerciseHelper.Truncate(expectedAnswer),
                GivenAnswer = SentenceExerciseHelper.Truncate(givenAnswer),
                IsCorrect = isCorrect,
                AnsweredAt = DateTime.UtcNow
            });

            await _attemptRepository.SaveChangesAsync();
        }

        private static PrepositionExerciseModel ToExerciseModel(PrepositionExercise exercise, PrepositionAttemptSample lastAttempt)
        {
            var slots = SplitSlots(exercise.Answers);

            return new PrepositionExerciseModel
            {
                Id = exercise.Id,
                ExerciseType = exercise.ExerciseType,
                Usage = exercise.Usage,
                CefrLevel = exercise.CefrLevel,
                Sentence = exercise.Sentence,
                Translation = exercise.Translation,
                BlankCount = slots.Count,
                IsPriorityReview = lastAttempt is { IsCorrect: false },
                ShownPrepositions = exercise.ExerciseType == PrepositionExerciseType.FindTheMistake
                    ? SplitShown(exercise.ShownPrepositions)
                    : new List<string>()
            };
        }

        private static int GetPriorityBucket(PrepositionAttemptSample lastAttempt)
        {
            if (lastAttempt is null)
            {
                return 1;
            }

            return lastAttempt.IsCorrect ? 2 : 0;
        }

        // Todas as frases aceitas: cada combinação de alternativas das lacunas mais as
        // frases alternativas cadastradas no exercício.
        private static List<string> BuildAcceptedSentences(PrepositionExercise exercise, List<List<string>> slots)
        {
            var combinations = new List<List<string>> { new() };

            foreach (var alternatives in slots)
            {
                combinations = combinations
                    .SelectMany(combination => alternatives.Select(alternative => combination.Append(alternative).ToList()))
                    .Take(MaxAcceptedCombinations)
                    .ToList();
            }

            var accepted = combinations.Select(combination => Fill(exercise.Sentence, combination)).ToList();

            if (!string.IsNullOrWhiteSpace(exercise.AlternativeSentences))
            {
                accepted.AddRange(exercise.AlternativeSentences
                    .Split(SlotSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            }

            return accepted;
        }

        private static List<string> ExtractPrepositions(string sentence)
        {
            return Regex.Matches(sentence ?? string.Empty, @"[\p{L}']+")
                .Select(m => m.Value.ToLowerInvariant())
                .Where(TrackedPrepositions.Contains)
                .ToList();
        }

        private static string Fill(string template, IReadOnlyList<string> values)
        {
            var parts = template.Split(BlankToken);
            var builder = new System.Text.StringBuilder(parts[0]);

            for (var i = 1; i < parts.Length; i++)
            {
                var value = i - 1 < values.Count ? values[i - 1] : string.Empty;

                // Uma lacuna no início da frase recebe a preposição com inicial maiúscula.
                if (string.IsNullOrWhiteSpace(builder.ToString()) && value.Length > 0)
                {
                    value = char.ToUpperInvariant(value[0]) + value[1..];
                }

                builder.Append(value).Append(parts[i]);
            }

            return builder.ToString();
        }

        private static List<List<string>> SplitSlots(string answers)
        {
            return (answers ?? string.Empty)
                .Split(SlotSeparator, StringSplitOptions.TrimEntries)
                .Select(slot => slot
                    .Split(AlternativeSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList())
                .Where(alternatives => alternatives.Count > 0)
                .ToList();
        }

        private static List<string> SplitShown(string shownPrepositions)
        {
            return string.IsNullOrWhiteSpace(shownPrepositions)
                ? new List<string>()
                : shownPrepositions.Split(SlotSeparator, StringSplitOptions.TrimEntries).ToList();
        }

        private static string NormalizeWord(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().Trim('.', ',', '!', '?', ';', ':', '"').ToLowerInvariant();
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
