using System.Text.RegularExpressions;
using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class SectionTestService : ISectionTestService
    {
        private const int QuestionCount = 50;
        private const int MinQuestionCount = 10;
        private const int PassPercentage = 90;

        // Metade do teste é reservada para a gramática obrigatória da seção.
        private const double MandatoryGrammarShare = 0.5;

        // Peso mínimo (ver GetWordWeight) para uma palavra poder ser escondida na lacuna.
        private const double ContentWordWeight = 0.5;

        // Quantidade de palavras de fora da frase oferecidas como possíveis opções erradas da lacuna.
        private const int DistractorPoolSize = 60;

        private static readonly TimeSpan MarkerTimeout = TimeSpan.FromMilliseconds(200);
        private static readonly Regex TokenPattern = new(@"[\p{L}']+", RegexOptions.Compiled);

        // Frases de gramática priorizam a produção em inglês, que mostra se a estrutura foi entendida.
        private static readonly ExerciseType[] GrammarRotation =
        {
            ExerciseType.TranslationToTarget,
            ExerciseType.TranslationToTarget,
            ExerciseType.TranslationToNative
        };

        private static readonly ExerciseType[] VocabularyRotation =
        {
            ExerciseType.TranslationToTarget,
            ExerciseType.FillInTheBlank,
            ExerciseType.TranslationToNative
        };

        private readonly ICourseService _courseService;
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ISentenceRepository _sentenceRepository;
        private readonly IWordRepository _wordRepository;
        private readonly IStudySessionRepository _studySessionRepository;
        private readonly IExerciseAttemptRepository _attemptRepository;
        private readonly IWordPerformanceService _performanceService;
        private readonly IUserRepository _userRepository;
        private readonly IUserNodeProgressRepository _nodeProgressRepository;

        public SectionTestService(
            ICourseService courseService,
            ICourseRepository courseRepository,
            ILessonRepository lessonRepository,
            ISentenceRepository sentenceRepository,
            IWordRepository wordRepository,
            IStudySessionRepository studySessionRepository,
            IExerciseAttemptRepository attemptRepository,
            IWordPerformanceService performanceService,
            IUserRepository userRepository,
            IUserNodeProgressRepository nodeProgressRepository)
        {
            _courseService = courseService;
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _sentenceRepository = sentenceRepository;
            _wordRepository = wordRepository;
            _studySessionRepository = studySessionRepository;
            _attemptRepository = attemptRepository;
            _performanceService = performanceService;
            _userRepository = userRepository;
            _nodeProgressRepository = nodeProgressRepository;
        }

        public async Task<SectionTestSessionModel> StartAsync(int? userId, int sectionId)
        {
            var user = await ResolveUserAsync(userId)
                ?? throw new InvalidOperationException("Nenhum usuário disponível para iniciar o teste.");

            var (target, source) = await ResolveSkipAsync(user.Id, sectionId);

            var sourceSection = await _courseRepository.GetSectionDetailsAsync(source.Id)
                ?? throw new KeyNotFoundException("Seção atual não encontrada.");

            var (exercises, grammarTopics) = await BuildExercisesAsync(sourceSection);

            if (exercises.Count < MinQuestionCount)
            {
                throw new InvalidOperationException("A seção atual ainda não tem frases suficientes para montar o teste.");
            }

            var session = await _studySessionRepository.StartSectionTestSessionAsync(
                user.Id,
                sourceSection.Course.LanguageId,
                target.Id,
                exercises.Count);

            var requiredCorrect = GetRequiredCorrect(exercises.Count);

            return new SectionTestSessionModel
            {
                StudySessionId = session.Id,
                SectionId = target.Id,
                SectionTitle = target.Title,
                SourceSectionId = sourceSection.Id,
                SourceSectionTitle = sourceSection.Title,
                CefrLevel = sourceSection.CefrLevel,
                TotalExercises = exercises.Count,
                RequiredCorrect = requiredCorrect,
                MaxWrong = exercises.Count - requiredCorrect,
                PassPercentage = PassPercentage,
                GrammarTopics = grammarTopics,
                Exercises = exercises
            };
        }

        public async Task<LessonAnswerResultModel> AnswerAsync(LessonAnswerModel answer)
        {
            var user = await ResolveUserAsync(answer.UserId)
                ?? throw new InvalidOperationException("Nenhum usuário disponível para registrar a resposta.");

            var session = await _studySessionRepository.GetByIdAsync(answer.StudySessionId);

            if (session is null || session.UserId != user.Id || !session.SectionId.HasValue)
            {
                throw new KeyNotFoundException("Teste da seção não encontrado.");
            }

            if (session.Status != ProgressStatus.InProgress)
            {
                throw new InvalidOperationException("Este teste já foi encerrado.");
            }

            var total = session.ExerciseCount ?? 0;

            if (session.WrongCount > total - GetRequiredCorrect(total))
            {
                throw new InvalidOperationException("Você já errou mais questões do que o permitido neste teste.");
            }

            if (session.CorrectCount + session.WrongCount >= total)
            {
                throw new InvalidOperationException("Todas as questões do teste já foram respondidas.");
            }

            if (answer.ExerciseType is not (ExerciseType.TranslationToNative or ExerciseType.TranslationToTarget or ExerciseType.FillInTheBlank))
            {
                throw new ArgumentException("Tipo de exercício não suportado no teste.", nameof(answer));
            }

            var sentence = await _sentenceRepository.GetByIdAsync(answer.SentenceId)
                ?? throw new KeyNotFoundException("Frase não encontrada.");

            if (sentence.LanguageId != session.LanguageId)
            {
                throw new ArgumentException("A frase não pertence ao idioma do teste.", nameof(answer));
            }

            // Cada questão vale uma única vez: repetir a mesma resposta não aumenta a pontuação.
            if (await _attemptRepository.ExistsInSessionAsync(session.Id, sentence.Id, answer.ExerciseType))
            {
                throw new InvalidOperationException("Esta questão já foi respondida.");
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

        public async Task<SectionTestResultModel> CompleteAsync(int? userId, int studySessionId)
        {
            var user = await ResolveUserAsync(userId)
                ?? throw new InvalidOperationException("Nenhum usuário disponível para concluir o teste.");

            var session = await _studySessionRepository.GetByIdAsync(studySessionId);

            if (session is null || session.UserId != user.Id || !session.SectionId.HasValue)
            {
                throw new KeyNotFoundException("Teste da seção não encontrado.");
            }

            var total = session.ExerciseCount ?? 0;
            var requiredCorrect = GetRequiredCorrect(total);
            var failed = session.WrongCount > total - requiredCorrect;
            var passed = !failed && session.CorrectCount >= requiredCorrect;
            var skippedLessons = 0;

            if (session.Status == ProgressStatus.InProgress)
            {
                // O teste pode ser encerrado antes do fim apenas quando já não há como atingir a nota mínima.
                if (!failed && session.CorrectCount + session.WrongCount < total)
                {
                    throw new InvalidOperationException("Responda todas as questões antes de concluir o teste.");
                }

                var now = DateTime.UtcNow;

                // A seção é liberada antes de fechar a sessão: se algo falhar, o teste pode ser concluído de novo.
                if (passed)
                {
                    skippedLessons = await UnlockSectionAsync(user.Id, session.LanguageId, session.SectionId.Value, now);
                }

                session.Status = ProgressStatus.Completed;
                session.FinishedAt = now;
                session.XpEarned = 0;

                await _studySessionRepository.SaveChangesAsync();
            }
            else if (session.Status != ProgressStatus.Completed)
            {
                throw new InvalidOperationException("Este teste foi encerrado sem ser concluído.");
            }

            var section = await _courseRepository.GetSectionDetailsAsync(session.SectionId.Value);
            var answered = session.CorrectCount + session.WrongCount;

            return new SectionTestResultModel
            {
                StudySessionId = session.Id,
                SectionId = session.SectionId.Value,
                SectionTitle = section?.Title,
                Passed = passed,
                CorrectCount = session.CorrectCount,
                WrongCount = session.WrongCount,
                TotalExercises = total,
                RequiredCorrect = requiredCorrect,
                AccuracyPercentage = answered == 0
                    ? 0
                    : (int)Math.Round((double)session.CorrectCount / answered * 100, MidpointRounding.AwayFromZero),
                SkippedLessons = skippedLessons
            };
        }

        // Só é possível pular para a seção imediatamente seguinte à que o usuário está estudando.
        private async Task<(SectionModel Target, SectionModel Source)> ResolveSkipAsync(int userId, int targetSectionId)
        {
            var sections = (await _courseService.GetTrackAsync(userId))
                .OrderBy(c => c.Position)
                .SelectMany(c => c.Sections.OrderBy(s => s.Position))
                .ToList();

            var index = sections.FindIndex(s => s.Id == targetSectionId);

            if (index < 0)
            {
                throw new KeyNotFoundException("Seção não encontrada.");
            }

            var target = sections[index];

            if (index == 0 || target.Status != ProgressStatus.Locked)
            {
                throw new InvalidOperationException("Esta seção já está liberada.");
            }

            var source = sections[index - 1];

            if (source.Status is not (ProgressStatus.Available or ProgressStatus.InProgress))
            {
                throw new InvalidOperationException("Só é possível pular para a seção seguinte à que você está estudando.");
            }

            return (target, source);
        }

        // Monta as questões a partir das frases do nível da seção atual, em duas etapas:
        // 1) metade do teste cobre a gramática obrigatória, alternando entre as formas de cada tópico
        //    (afirmativa, negativa, pergunta...) e preferindo as frases mais difíceis;
        // 2) o restante é escolhido de forma gulosa, combinando a dificuldade da frase (tamanho,
        //    quantidade de palavras do currículo e estruturas gramaticais) com as palavras mais
        //    importantes da seção que ainda não apareceram no teste.
        private async Task<(List<LessonExerciseModel> Exercises, List<string> GrammarTopics)> BuildExercisesAsync(Section source)
        {
            var languageId = source.Course.LanguageId;
            var curriculum = (await _lessonRepository.GetCourseLessonWordsAsync(source.CourseId)).ToList();

            // Seção em que cada palavra do currículo é apresentada pela primeira vez.
            var introducedIn = curriculum
                .GroupBy(lw => lw.WordId)
                .ToDictionary(g => g.Key, g => g.Min(lw => lw.Lesson.PathNode.Unit.Section.Position));

            var vocabulary = curriculum
                .Where(lw => introducedIn[lw.WordId] <= source.Position)
                .Select(lw => lw.Word)
                .GroupBy(w => w.Id)
                .Select(g => g.First())
                .ToList();

            var allWords = (await _wordRepository.GetByLanguageAsync(languageId)).ToDictionary(w => w.Id);

            // Palavras da seção: as ensinadas nas lições da seção e todas as palavras do nível CEFR
            // da seção (mesmo as que ainda não estão em nenhuma lição), exceto as que o currículo só
            // apresenta em seções posteriores.
            var sectionWordIds = curriculum
                .Where(lw => lw.Lesson.PathNode.Unit.SectionId == source.Id)
                .Select(lw => lw.WordId)
                .ToHashSet();

            sectionWordIds.UnionWith(allWords.Values
                .Where(w => w.CefrLevel == source.CefrLevel
                    && (!introducedIn.TryGetValue(w.Id, out var position) || position <= source.Position))
                .Select(w => w.Id));

            vocabulary = vocabulary
                .Concat(sectionWordIds.Where(allWords.ContainsKey).Select(id => allWords[id]))
                .GroupBy(w => w.Id)
                .Select(g => g.First())
                .ToList();

            var topics = (source.GrammarTopics ?? new List<GrammarTopic>())
                .OrderByDescending(g => g.IsMandatory)
                .ThenBy(g => g.Position)
                .Select(g => new TopicMarkers(g, ParseMarkers(g.Markers)))
                .Where(t => t.Markers.Count > 0)
                .ToList();

            // Frases do nível da seção e dos níveis anteriores (que contenham palavras da seção: das
            // lições da seção ou do nível CEFR dela). Frases com palavras que o currículo só ensina em
            // seções posteriores ficam de fora.
            var sentences = new List<Sentence>();

            foreach (var level in Enum.GetValues<CefrLevel>().Where(l => l <= source.CefrLevel))
            {
                sentences.AddRange(await _sentenceRepository.GetByLevelAsync(languageId, level));
            }

            var candidates = sentences
                .Select(s =>
                {
                    var sentenceWordIds = (s.SentenceWords ?? new List<SentenceWord>())
                        .Select(sw => sw.WordId)
                        .Distinct()
                        .ToList();

                    return new Candidate
                    {
                        Sentence = s,
                        WordIds = sentenceWordIds.Where(introducedIn.ContainsKey).ToList(),
                        SectionWordIds = sentenceWordIds.Where(sectionWordIds.Contains).ToList(),
                        AllWordIds = sentenceWordIds,
                        TokenCount = TokenPattern.Matches(s.Text ?? string.Empty).Count,
                        Tiebreaker = Random.Shared.NextDouble()
                    };
                })
                .Where(c => c.SectionWordIds.Count > 0
                    && c.WordIds.All(id => introducedIn[id] <= source.Position))
                .ToList();

            if (candidates.Count == 0)
            {
                return (new List<LessonExerciseModel>(), new List<string>());
            }

            // Importância da palavra: em quantas frases da seção ela aparece, com peso menor para
            // palavras gramaticais (artigos, pronomes...), que aparecem em quase todas as frases.
            var weightedFrequency = candidates
                .SelectMany(c => c.SectionWordIds)
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count() * GetWordWeight(allWords.GetValueOrDefault(g.Key)));

            var maxFrequency = Math.Max(weightedFrequency.Values.Max(), 1e-6);
            var importance = weightedFrequency.ToDictionary(kv => kv.Key, kv => kv.Value / maxFrequency);

            foreach (var candidate in candidates)
            {
                candidate.MatchedMarkers = topics.ToDictionary(
                    t => t.Topic.Id,
                    t => Enumerable.Range(0, t.Markers.Count).Where(i => IsMatch(t.Markers[i], candidate.Sentence.Text)).ToList());

                var matchedTopics = candidate.MatchedMarkers.Count(kv => kv.Value.Count > 0);

                candidate.Difficulty = candidate.TokenCount
                    + 1.5 * candidate.WordIds.Union(candidate.SectionWordIds).Count()
                    + 2.0 * matchedTopics;

                candidate.Importance = candidate.SectionWordIds.Sum(id => importance[id]);
            }

            var maxDifficulty = candidates.Max(c => c.Difficulty);
            var maxImportance = Math.Max(candidates.Max(c => c.Importance), 1e-6);

            foreach (var candidate in candidates)
            {
                candidate.Difficulty /= maxDifficulty;
                candidate.Importance /= maxImportance;
            }

            var selected = new List<(Candidate Candidate, bool IsGrammar)>();
            var used = new HashSet<int>();
            var covered = new HashSet<int>();
            var coveredTopics = new List<string>();

            var mandatory = topics.Where(t => t.Topic.IsMandatory).ToList();
            var grammarSlots = (int)Math.Round(QuestionCount * MandatoryGrammarShare);

            for (var t = 0; t < mandatory.Count; t++)
            {
                var topic = mandatory[t];
                var quota = grammarSlots / mandatory.Count + (t < grammarSlots % mandatory.Count ? 1 : 0);

                // Uma fila por forma do tópico, para que todas apareçam no teste.
                var buckets = Enumerable.Range(0, topic.Markers.Count)
                    .Select(m => new Queue<Candidate>(candidates
                        .Where(c => c.MatchedMarkers[topic.Topic.Id].Contains(m))
                        .OrderByDescending(c => 2 * c.Difficulty + c.Importance + 0.1 * c.Tiebreaker)))
                    .Where(q => q.Count > 0)
                    .ToList();

                var taken = 0;

                while (taken < quota && buckets.Any(q => q.Count > 0))
                {
                    foreach (var bucket in buckets)
                    {
                        if (taken >= quota)
                        {
                            break;
                        }

                        while (bucket.Count > 0)
                        {
                            var candidate = bucket.Dequeue();

                            if (used.Add(candidate.Sentence.Id))
                            {
                                selected.Add((candidate, true));
                                covered.UnionWith(candidate.SectionWordIds);
                                taken++;
                                break;
                            }
                        }
                    }
                }

                if (taken > 0)
                {
                    coveredTopics.Add(topic.Topic.Title);
                }
            }

            while (selected.Count < QuestionCount)
            {
                var best = candidates
                    .Where(c => !used.Contains(c.Sentence.Id))
                    .Select(c => new
                    {
                        Candidate = c,
                        Score = 2 * c.Difficulty
                            + c.SectionWordIds.Where(id => !covered.Contains(id)).Sum(id => importance[id])
                            + 0.1 * c.Tiebreaker
                    })
                    .OrderByDescending(x => x.Score)
                    .FirstOrDefault();

                if (best is null)
                {
                    break;
                }

                used.Add(best.Candidate.Sentence.Id);
                selected.Add((best.Candidate, false));
                covered.UnionWith(best.Candidate.SectionWordIds);
            }

            var contentVocabulary = vocabulary.Where(w => GetWordWeight(w) >= ContentWordWeight).ToList();
            var exercises = CreateExercises(selected, contentVocabulary, allWords, importance);

            return (exercises, coveredTopics);
        }

        // Distribui os tipos de exercício pelas frases escolhidas. Se faltarem frases para completar
        // o teste, cada frase volta com um tipo de exercício ainda não usado.
        private static List<LessonExerciseModel> CreateExercises(
            List<(Candidate Candidate, bool IsGrammar)> selected,
            List<Word> vocabulary,
            Dictionary<int, Word> allWords,
            Dictionary<int, double> importance)
        {
            var exercises = new List<LessonExerciseModel>();
            var contentWordIds = vocabulary.Select(w => w.Id).ToHashSet();
            var usedTypes = selected.ToDictionary(s => s.Candidate.Sentence.Id, _ => new HashSet<ExerciseType>());
            var round = 0;

            while (exercises.Count < QuestionCount && selected.Count > 0)
            {
                var addedInRound = 0;

                for (var i = 0; i < selected.Count && exercises.Count < QuestionCount; i++)
                {
                    var (candidate, isGrammar) = selected[i];
                    var rotation = isGrammar ? GrammarRotation : VocabularyRotation;

                    // O vocabulário da lacuna fica restrito às palavras da frase mais uma amostra das
                    // demais (para as opções erradas), evitando procurar centenas de palavras em cada frase.
                    var sentenceVocabulary = candidate.AllWordIds
                        .Where(contentWordIds.Contains)
                        .Select(id => allWords[id])
                        .Concat(vocabulary
                            .Where(w => !candidate.AllWordIds.Contains(w.Id))
                            .OrderBy(_ => Random.Shared.Next())
                            .Take(DistractorPoolSize))
                        .ToList();

                    var exercise = CreateExercise(candidate.Sentence, rotation, (i + round) % rotation.Length, usedTypes[candidate.Sentence.Id], sentenceVocabulary, importance);

                    if (exercise is null)
                    {
                        continue;
                    }

                    exercises.Add(exercise);
                    addedInRound++;
                }

                if (addedInRound == 0)
                {
                    break;
                }

                round++;
            }

            SentenceExerciseHelper.Shuffle(exercises);

            for (var i = 0; i < exercises.Count; i++)
            {
                exercises[i].Position = i + 1;
            }

            return exercises;
        }

        private static LessonExerciseModel CreateExercise(
            Sentence sentence,
            ExerciseType[] rotation,
            int preferredIndex,
            HashSet<ExerciseType> usedTypes,
            List<Word> vocabulary,
            Dictionary<int, double> importance)
        {
            for (var offset = 0; offset < rotation.Length; offset++)
            {
                var type = rotation[(preferredIndex + offset) % rotation.Length];

                if (usedTypes.Contains(type))
                {
                    continue;
                }

                // Na lacuna, a palavra escondida é a palavra de conteúdo (substantivo, verbo, adjetivo...)
                // mais importante da seção presente na frase; frases só com palavras gramaticais
                // (artigos, pronomes...) recebem outro tipo de exercício.
                var exercise = type == ExerciseType.FillInTheBlank
                    ? SentenceExerciseHelper.CreateFillInTheBlank(sentence, vocabulary, w => importance.GetValueOrDefault(w.Id) + GetWordWeight(w))
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

        // Ao passar no teste, todas as lições das seções anteriores à seção alvo passam a contar como
        // concluídas (sem XP), o que libera a seção alvo na trilha.
        private async Task<int> UnlockSectionAsync(int userId, int languageId, int targetSectionId, DateTime now)
        {
            var sections = (await _courseRepository.GetActiveTrackAsync(languageId))
                .OrderBy(c => c.Position)
                .SelectMany(c => (c.Sections ?? new List<Section>()).OrderBy(s => s.Position))
                .ToList();

            var index = sections.FindIndex(s => s.Id == targetSectionId);

            if (index <= 0)
            {
                return 0;
            }

            var nodes = sections
                .Take(index)
                .SelectMany(s => s.Units ?? new List<Unit>())
                .SelectMany(u => u.PathNodes ?? new List<PathNode>())
                .ToList();

            var completed = (await _studySessionRepository.GetCompletedLessonIdsAsync(userId)).ToHashSet();

            var pending = nodes
                .SelectMany(n => n.Lessons ?? new List<Lesson>())
                .Select(l => l.Id)
                .Where(id => !completed.Contains(id))
                .Distinct()
                .ToList();

            await _studySessionRepository.AddSkippedLessonSessionsAsync(userId, languageId, pending, now);

            foreach (var node in nodes)
            {
                var progress = await _nodeProgressRepository.GetAsync(userId, node.Id);

                if (progress is null)
                {
                    progress = new UserNodeProgress
                    {
                        UserId = userId,
                        PathNodeId = node.Id
                    };

                    await _nodeProgressRepository.AddAsync(progress);
                }

                progress.CompletedLessonsCount = node.Lessons?.Count ?? 0;

                if (!progress.IsCompleted)
                {
                    progress.IsCompleted = true;
                    progress.CompletedAt = now;
                }
            }

            await _nodeProgressRepository.SaveChangesAsync();
            await _userRepository.ApplySectionSkipAsync(userId, languageId, index + 1, pending.Count);

            return pending.Count;
        }

        private static double GetWordWeight(Word word)
        {
            return word?.PartOfSpeech switch
            {
                PartOfSpeech.Article or PartOfSpeech.Pronoun or PartOfSpeech.Preposition
                    or PartOfSpeech.Conjunction or PartOfSpeech.Interjection => 0.15,
                PartOfSpeech.Numeral => 0.6,
                null => 0.5,
                _ => 1.0
            };
        }

        private static int GetRequiredCorrect(int total)
        {
            return (int)Math.Ceiling(total * PassPercentage / 100.0);
        }

        private static List<Regex> ParseMarkers(string markers)
        {
            var result = new List<Regex>();

            if (string.IsNullOrWhiteSpace(markers))
            {
                return result;
            }

            foreach (var pattern in markers.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                try
                {
                    result.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, MarkerTimeout));
                }
                catch (ArgumentException)
                {
                    // Marcador inválido no conteúdo: é ignorado para não impedir o teste.
                }
            }

            return result;
        }

        private static bool IsMatch(Regex marker, string text)
        {
            try
            {
                return !string.IsNullOrEmpty(text) && marker.IsMatch(text);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private async Task<User> ResolveUserAsync(int? userId)
        {
            return userId is > 0
                ? await _userRepository.GetWithProgressesAsync(userId.Value)
                : await _userRepository.GetDefaultAsync();
        }

        private sealed record TopicMarkers(GrammarTopic Topic, List<Regex> Markers);

        private sealed class Candidate
        {
            public Sentence Sentence { get; init; }
            public List<int> WordIds { get; init; }
            public List<int> SectionWordIds { get; init; }
            public List<int> AllWordIds { get; init; }
            public int TokenCount { get; init; }
            public double Tiebreaker { get; init; }
            public Dictionary<int, List<int>> MatchedMarkers { get; set; }
            public double Difficulty { get; set; }
            public double Importance { get; set; }
        }
    }
}
