using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class LessonService : ILessonService
    {
        private const int ExercisesPerLesson = 15;

        // Ordem em que os tipos de exercício se alternam entre as frases da lição.
        private static readonly ExerciseType[] ExerciseRotation =
        {
            ExerciseType.TranslationToNative,
            ExerciseType.TranslationToTarget,
            ExerciseType.FillInTheBlank
        };

        private readonly ILessonRepository _lessonRepository;
        private readonly ISentenceRepository _sentenceRepository;
        private readonly IWordRepository _wordRepository;
        private readonly IStudySessionRepository _studySessionRepository;
        private readonly IExerciseAttemptRepository _attemptRepository;
        private readonly IWordPerformanceService _performanceService;
        private readonly IUserRepository _userRepository;
        private readonly IUserNodeProgressRepository _nodeProgressRepository;
        private readonly ICourseService _courseService;

        public LessonService(
            ILessonRepository lessonRepository,
            ISentenceRepository sentenceRepository,
            IWordRepository wordRepository,
            IStudySessionRepository studySessionRepository,
            IExerciseAttemptRepository attemptRepository,
            IWordPerformanceService performanceService,
            IUserRepository userRepository,
            IUserNodeProgressRepository nodeProgressRepository,
            ICourseService courseService)
        {
            _lessonRepository = lessonRepository;
            _sentenceRepository = sentenceRepository;
            _wordRepository = wordRepository;
            _studySessionRepository = studySessionRepository;
            _attemptRepository = attemptRepository;
            _performanceService = performanceService;
            _userRepository = userRepository;
            _nodeProgressRepository = nodeProgressRepository;
            _courseService = courseService;
        }

        public async Task<LessonSessionModel> StartAsync(int? userId, int pathNodeId)
        {
            var user = await ResolveUserAsync(userId)
                ?? throw new InvalidOperationException("Nenhum usuário disponível para iniciar a lição.");

            var node = await _lessonRepository.GetNodeWithContextAsync(pathNodeId)
                ?? throw new KeyNotFoundException("Nó da trilha não encontrado.");

            // O status calculado para a trilha é a fonte da verdade sobre o que está liberado.
            var track = await _courseService.GetTrackAsync(user.Id);
            var nodeModel = track
                .SelectMany(c => c.Sections)
                .SelectMany(s => s.Units)
                .SelectMany(u => u.PathNodes)
                .FirstOrDefault(pn => pn.Id == pathNodeId);

            if (nodeModel is null || nodeModel.Status == ProgressStatus.Locked)
            {
                throw new InvalidOperationException("Este nó ainda está bloqueado. Conclua os nós anteriores para liberá-lo.");
            }

            // Continua da primeira lição não concluída; com o nó completo, sorteia uma lição para revisão.
            var lessonModel = nodeModel.Lessons
                .OrderBy(l => l.Position)
                .FirstOrDefault(l => l.Status is ProgressStatus.Available or ProgressStatus.InProgress);

            var isReview = lessonModel is null;

            lessonModel ??= nodeModel.Lessons.Count > 0
                ? nodeModel.Lessons[Random.Shared.Next(nodeModel.Lessons.Count)]
                : null;

            var lesson = lessonModel is null ? null : node.Lessons.FirstOrDefault(l => l.Id == lessonModel.Id);

            if (lesson is null)
            {
                throw new InvalidOperationException("Este nó ainda não possui lições.");
            }

            var exercises = await BuildExercisesAsync(user.Id, node, lesson);

            if (exercises.Count == 0)
            {
                throw new InvalidOperationException("Esta lição ainda não possui frases cadastradas.");
            }

            var session = await _studySessionRepository.StartLessonSessionAsync(user.Id, node.Unit.Section.Course.LanguageId, lesson.Id);

            return new LessonSessionModel
            {
                StudySessionId = session.Id,
                LessonId = lesson.Id,
                PathNodeId = node.Id,
                SectionId = node.Unit.SectionId,
                UnitId = node.UnitId,
                UnitTitle = node.Unit.Title,
                CefrLevel = node.Unit.Section.CefrLevel,
                LessonPosition = lesson.Position,
                TotalLessons = nodeModel.TotalLessons,
                CompletedLessons = nodeModel.CompletedLessons,
                XpReward = lesson.XpReward,
                IsReview = isReview,
                Exercises = exercises
            };
        }

        public async Task<LessonAnswerResultModel> AnswerAsync(LessonAnswerModel answer)
        {
            var user = await ResolveUserAsync(answer.UserId)
                ?? throw new InvalidOperationException("Nenhum usuário disponível para registrar a resposta.");

            var session = await GetOpenSessionAsync(user, answer.StudySessionId);

            var sentence = await _sentenceRepository.GetByIdAsync(answer.SentenceId)
                ?? throw new KeyNotFoundException("Frase não encontrada.");

            if (sentence.LanguageId != session.LanguageId)
            {
                throw new ArgumentException("A frase não pertence ao idioma da lição.", nameof(answer));
            }

            if (answer.ExerciseType is not (ExerciseType.TranslationToNative or ExerciseType.TranslationToTarget or ExerciseType.FillInTheBlank))
            {
                throw new ArgumentException("Tipo de exercício não suportado nas lições.", nameof(answer));
            }

            var word = answer.ExerciseType == ExerciseType.FillInTheBlank && answer.BlankWordId.HasValue
                ? await _wordRepository.GetByIdAsync(answer.BlankWordId.Value)
                : null;

            var (correctAnswer, acceptedAnswers) = SentenceExerciseHelper.GetExpectedAnswers(sentence, answer.ExerciseType, word);
            int? wordId = answer.ExerciseType == ExerciseType.FillInTheBlank ? word.Id : null;

            var isCorrect = SentenceExerciseHelper.IsCorrect(answer.Answer, acceptedAnswers, SentenceExerciseHelper.AcceptsMissingApostrophes(user));

            var answeredAt = DateTime.UtcNow;

            await _attemptRepository.AddAsync(new ExerciseAttempt
            {
                StudySessionId = session.Id,
                SentenceId = sentence.Id,
                WordId = wordId,
                ExerciseType = answer.ExerciseType,
                GivenAnswer = SentenceExerciseHelper.Truncate(answer.Answer?.Trim()),
                ExpectedAnswer = SentenceExerciseHelper.Truncate(correctAnswer),
                IsCorrect = isCorrect,
                ResponseTimeMs = answer.ResponseTimeMs,
                AnsweredAt = answeredAt
            });

            if (wordId.HasValue)
            {
                // Exercícios ligados a uma palavra alimentam o desempenho dela (dicionário e revisão).
                await _performanceService.ApplyAttemptAsync(user.Id, wordId.Value, isCorrect, answeredAt);
            }

            if (isCorrect)
            {
                session.CorrectCount++;
            }
            else
            {
                session.WrongCount++;
            }

            // A sessão, a tentativa e o desempenho da palavra são rastreados pelo mesmo contexto,
            // então um único SaveChanges grava tudo.
            await _attemptRepository.SaveChangesAsync();

            return new LessonAnswerResultModel
            {
                IsCorrect = isCorrect,
                SubmittedAnswer = answer.Answer?.Trim(),
                CorrectAnswer = correctAnswer,
                SentenceText = sentence.Text,
                SentenceTranslation = sentence.Translation
            };
        }

        public async Task<LessonCompletionModel> CompleteAsync(int? userId, int studySessionId)
        {
            var user = await ResolveUserAsync(userId)
                ?? throw new InvalidOperationException("Nenhum usuário disponível para concluir a lição.");

            var session = await _studySessionRepository.GetByIdAsync(studySessionId);

            if (session is null || session.UserId != user.Id || !session.LessonId.HasValue)
            {
                throw new KeyNotFoundException("Sessão da lição não encontrada.");
            }

            var lesson = await _lessonRepository.GetWithContextAsync(session.LessonId.Value)
                ?? throw new KeyNotFoundException("Lição não encontrada.");

            if (session.Status == ProgressStatus.InProgress)
            {
                // Cada frase errada volta para o fim da fila até ser acertada,
                // por isso a lição só termina com todas as frases respondidas corretamente.
                if (session.CorrectCount < ExercisesPerLesson)
                {
                    throw new InvalidOperationException("Responda todas as frases antes de concluir a lição.");
                }

                var completedBefore = (await _studySessionRepository.GetCompletedLessonIdsAsync(user.Id)).Contains(lesson.Id);
                var now = DateTime.UtcNow;

                session.Status = ProgressStatus.Completed;
                session.FinishedAt = now;
                session.XpEarned = lesson.XpReward;

                await _studySessionRepository.SaveChangesAsync();
                await _userRepository.AddLessonRewardAsync(user.Id, session.LanguageId, session.XpEarned, !completedBefore);
                await UpdateNodeProgressAsync(user.Id, lesson, now);
            }
            else if (session.Status != ProgressStatus.Completed)
            {
                throw new InvalidOperationException("Esta sessão foi encerrada sem concluir a lição.");
            }

            var completedIds = (await _studySessionRepository.GetCompletedLessonIdsAsync(user.Id)).ToHashSet();
            var nodeLessons = lesson.PathNode.Lessons ?? new List<Lesson>();
            var completedLessons = nodeLessons.Count(l => completedIds.Contains(l.Id));
            var answered = session.CorrectCount + session.WrongCount;

            return new LessonCompletionModel
            {
                StudySessionId = session.Id,
                LessonId = lesson.Id,
                PathNodeId = lesson.PathNodeId,
                XpEarned = session.XpEarned,
                CorrectCount = session.CorrectCount,
                WrongCount = session.WrongCount,
                AccuracyPercentage = answered == 0
                    ? 0
                    : (int)Math.Round((double)session.CorrectCount / answered * 100, MidpointRounding.AwayFromZero),
                CompletedLessons = completedLessons,
                TotalLessons = nodeLessons.Count,
                NodeCompleted = nodeLessons.Count > 0 && completedLessons >= nodeLessons.Count
            };
        }

        private async Task UpdateNodeProgressAsync(int userId, Lesson lesson, DateTime now)
        {
            var completedIds = (await _studySessionRepository.GetCompletedLessonIdsAsync(userId)).ToHashSet();
            var nodeLessons = lesson.PathNode.Lessons ?? new List<Lesson>();
            var completedCount = nodeLessons.Count(l => completedIds.Contains(l.Id));

            var progress = await _nodeProgressRepository.GetAsync(userId, lesson.PathNodeId);

            if (progress is null)
            {
                progress = new UserNodeProgress
                {
                    UserId = userId,
                    PathNodeId = lesson.PathNodeId
                };

                await _nodeProgressRepository.AddAsync(progress);
            }

            progress.CompletedLessonsCount = completedCount;
            progress.LastPracticedAt = now;

            if (nodeLessons.Count > 0 && completedCount >= nodeLessons.Count && !progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = now;
            }

            await _nodeProgressRepository.SaveChangesAsync();
        }

        private async Task<StudySession> GetOpenSessionAsync(User user, int studySessionId)
        {
            var session = await _studySessionRepository.GetByIdAsync(studySessionId);

            if (session is null || session.UserId != user.Id || !session.LessonId.HasValue)
            {
                throw new KeyNotFoundException("Sessão da lição não encontrada.");
            }

            if (session.Status != ProgressStatus.InProgress)
            {
                throw new InvalidOperationException("Esta sessão da lição já foi encerrada.");
            }

            return session;
        }

        /// <summary>
        /// Monta as frases da lição a partir do vocabulário, sem vínculo manual entre frases e nós.
        /// Uma frase do nível CEFR da seção é elegível quando todas as palavras do currículo que ela
        /// contém já foram ensinadas até esta lição. Entram primeiro as que usam palavras da lição
        /// atual e, quando faltam frases, as de revisão (palavras mais recentes primeiro). Em ambos
        /// os grupos as frases que o usuário viu menos vezes têm prioridade, para variar as lições.
        /// </summary>
        private async Task<List<LessonExerciseModel>> BuildExercisesAsync(int userId, PathNode node, Lesson lesson)
        {
            var section = node.Unit.Section;
            var languageId = section.Course.LanguageId;
            var currentKey = (section.Position, node.Unit.Position, node.Position, lesson.Position);

            var curriculum = (await _lessonRepository.GetCourseLessonWordsAsync(section.CourseId)).ToList();

            // Momento (ordem na trilha) em que cada palavra do currículo é apresentada pela primeira vez.
            var introducedAt = curriculum
                .GroupBy(lw => lw.WordId)
                .ToDictionary(g => g.Key, g => g.Select(GetKey).Min());

            var introduced = curriculum
                .Where(lw => introducedAt[lw.WordId].CompareTo(currentKey) <= 0)
                .Select(lw => lw.Word)
                .GroupBy(w => w.Id)
                .Select(g => g.First())
                .ToList();

            var lessonWords = curriculum
                .Where(lw => lw.LessonId == lesson.Id)
                .Select(lw => lw.Word)
                .ToList();

            var lessonWordIds = lessonWords.Select(w => w.Id).ToHashSet();

            var sentences = await _sentenceRepository.GetByLevelAsync(languageId, section.CefrLevel);
            var seenCounts = await _attemptRepository.GetSentenceAttemptCountsAsync(userId, languageId);

            // Palavras fora do currículo (artigos, nomes, flexões etc.) não bloqueiam a frase.
            var eligible = sentences
                .Select(s => new
                {
                    Sentence = s,
                    CurriculumWordIds = (s.SentenceWords ?? new List<SentenceWord>())
                        .Select(sw => sw.WordId)
                        .Where(introducedAt.ContainsKey)
                        .Distinct()
                        .ToList(),
                    Seen = seenCounts.GetValueOrDefault(s.Id),
                    Tiebreaker = Random.Shared.Next()
                })
                .Where(c => c.CurriculumWordIds.Count > 0
                    && c.CurriculumWordIds.All(id => introducedAt[id].CompareTo(currentKey) <= 0))
                .ToList();

            var targets = eligible
                .Where(c => c.CurriculumWordIds.Any(lessonWordIds.Contains))
                .OrderBy(c => c.Seen)
                .ThenByDescending(c => c.CurriculumWordIds.Count(lessonWordIds.Contains))
                .ThenBy(c => c.Tiebreaker)
                .Select(c => c.Sentence)
                .Take(ExercisesPerLesson)
                .ToList();

            if (targets.Count < ExercisesPerLesson)
            {
                var reviews = eligible
                    .Where(c => !c.CurriculumWordIds.Any(lessonWordIds.Contains))
                    .OrderBy(c => c.Seen)
                    .ThenByDescending(c => c.CurriculumWordIds.Select(id => introducedAt[id]).Max())
                    .ThenBy(c => c.Tiebreaker)
                    .Select(c => c.Sentence)
                    .Take(ExercisesPerLesson - targets.Count);

                targets.AddRange(reviews);
            }

            return CreateExercises(SentenceExerciseHelper.Shuffle(targets), introduced, lessonWords);
        }

        /// <summary>
        /// Distribui os tipos de exercício em rodízio pelas frases. Se houver menos frases que
        /// exercícios, cada frase volta em outra rodada com um tipo de exercício diferente.
        /// </summary>
        private static List<LessonExerciseModel> CreateExercises(List<Sentence> sentences, List<Word> introduced, List<Word> lessonWords)
        {
            var exercises = new List<LessonExerciseModel>();

            if (sentences.Count == 0)
            {
                return exercises;
            }

            var usedTypes = sentences.ToDictionary(s => s.Id, _ => new HashSet<ExerciseType>());
            var round = 0;

            while (exercises.Count < ExercisesPerLesson)
            {
                var addedInRound = 0;

                for (var i = 0; i < sentences.Count && exercises.Count < ExercisesPerLesson; i++)
                {
                    var sentence = sentences[i];
                    var exercise = CreateExercise(sentence, (i + round) % ExerciseRotation.Length, usedTypes[sentence.Id], introduced, lessonWords);

                    if (exercise is null)
                    {
                        continue;
                    }

                    exercise.Position = exercises.Count + 1;
                    exercises.Add(exercise);
                    addedInRound++;
                }

                if (addedInRound == 0)
                {
                    // Todos os tipos possíveis já foram usados: libera a repetição de tipos.
                    if (usedTypes.Values.All(t => t.Count == 0))
                    {
                        break;
                    }

                    foreach (var types in usedTypes.Values)
                    {
                        types.Clear();
                    }
                }

                round++;
            }

            return exercises;
        }

        private static LessonExerciseModel CreateExercise(
            Sentence sentence,
            int preferredIndex,
            HashSet<ExerciseType> usedTypes,
            List<Word> introduced,
            List<Word> lessonWords)
        {
            for (var offset = 0; offset < ExerciseRotation.Length; offset++)
            {
                var type = ExerciseRotation[(preferredIndex + offset) % ExerciseRotation.Length];

                if (usedTypes.Contains(type))
                {
                    continue;
                }

                var exercise = type == ExerciseType.FillInTheBlank
                    ? CreateFillInTheBlank(sentence, introduced, lessonWords)
                    : SentenceExerciseHelper.CreateTranslation(sentence, type);

                if (exercise is null)
                {
                    continue;
                }

                usedTypes.Add(type);
                return exercise;
            }

            return null;
        }

        private static LessonExerciseModel CreateFillInTheBlank(Sentence sentence, List<Word> introduced, List<Word> lessonWords)
        {
            var lessonWordIds = lessonWords.Select(w => w.Id).ToHashSet();

            // A lacuna sempre é uma palavra já apresentada; as da lição atual têm prioridade.
            return SentenceExerciseHelper.CreateFillInTheBlank(sentence, introduced, w => lessonWordIds.Contains(w.Id) ? 1 : 0);
        }

        private static (int Section, int Unit, int Node, int Lesson) GetKey(LessonWord lessonWord)
        {
            var node = lessonWord.Lesson.PathNode;

            return (node.Unit.Section.Position, node.Unit.Position, node.Position, lessonWord.Lesson.Position);
        }

        private async Task<User> ResolveUserAsync(int? userId)
        {
            return userId is > 0
                ? await _userRepository.GetWithProgressesAsync(userId.Value)
                : await _userRepository.GetDefaultAsync();
        }
    }
}
