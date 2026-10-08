using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class CourseService : ICourseService
    {
        // Percentual de acerto a partir do qual a palavra conta como dominada no nível.
        private const int MasteryThreshold = 80;

        private readonly ICourseRepository _courseRepository;
        private readonly IStudySessionRepository _studySessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IPracticeService _practiceService;

        public CourseService(
            ICourseRepository courseRepository,
            IStudySessionRepository studySessionRepository,
            IUserRepository userRepository,
            ILanguageRepository languageRepository,
            IPracticeService practiceService)
        {
            _courseRepository = courseRepository;
            _studySessionRepository = studySessionRepository;
            _userRepository = userRepository;
            _languageRepository = languageRepository;
            _practiceService = practiceService;
        }

        public async Task<IEnumerable<CourseModel>> GetTrackAsync(int? userId)
        {
            var user = await ResolveUserAsync(userId);

            var languageId = ResolveLanguageId(user);
            var courses = await _courseRepository.GetActiveTrackAsync(languageId);

            var completed = new HashSet<int>();
            var inProgress = new HashSet<int>();

            if (user is not null)
            {
                completed = new HashSet<int>(await _studySessionRepository.GetCompletedLessonIdsAsync(user.Id));
                inProgress = new HashSet<int>(await _studySessionRepository.GetInProgressLessonIdsAsync(user.Id));
            }

            var models = new List<CourseModel>();
            var nextLessonAssigned = false;

            // A ordem de iteração (curso > seção > unidade > nó > lição) define a sequência da
            // trilha: a primeira lição não concluída depois das concluídas fica disponível.
            foreach (var course in courses.OrderBy(c => c.Position))
            {
                var courseModel = new CourseModel
                {
                    Id = course.Id,
                    LanguageId = course.LanguageId,
                    Name = course.Name,
                    Description = course.Description,
                    Position = course.Position,
                    CefrLevel = course.CefrLevel
                };

                foreach (var section in (course.Sections ?? new List<Section>()).OrderBy(s => s.Position))
                {
                    var sectionModel = new SectionModel
                    {
                        Id = section.Id,
                        CourseId = section.CourseId,
                        Title = section.Title,
                        Description = section.Description,
                        Position = section.Position,
                        CefrLevel = section.CefrLevel
                    };

                    foreach (var unit in (section.Units ?? new List<Unit>()).OrderBy(u => u.Position))
                    {
                        var unitModel = new UnitModel
                        {
                            Id = unit.Id,
                            SectionId = unit.SectionId,
                            Title = unit.Title,
                            Topic = unit.Topic,
                            Position = unit.Position
                        };

                        foreach (var node in (unit.PathNodes ?? new List<PathNode>()).OrderBy(pn => pn.Position))
                        {
                            var nodeModel = new PathNodeModel
                            {
                                Id = node.Id,
                                UnitId = node.UnitId,
                                NodeType = node.NodeType,
                                Position = node.Position
                            };

                            foreach (var lesson in (node.Lessons ?? new List<Lesson>()).OrderBy(l => l.Position))
                            {
                                nodeModel.Lessons.Add(new LessonModel
                                {
                                    Id = lesson.Id,
                                    PathNodeId = lesson.PathNodeId,
                                    Position = lesson.Position,
                                    XpReward = lesson.XpReward,
                                    Status = ResolveStatus(lesson.Id, completed, inProgress, ref nextLessonAssigned)
                                });
                            }

                            nodeModel.TotalLessons = nodeModel.Lessons.Count;
                            nodeModel.CompletedLessons = nodeModel.Lessons.Count(l => l.Status == ProgressStatus.Completed);
                            nodeModel.Status = Aggregate(nodeModel.Lessons.Select(l => l.Status));

                            unitModel.PathNodes.Add(nodeModel);
                        }

                        unitModel.Status = Aggregate(unitModel.PathNodes.Select(pn => pn.Status));
                        sectionModel.Units.Add(unitModel);
                    }

                    sectionModel.Status = Aggregate(sectionModel.Units.Select(u => u.Status));
                    courseModel.Sections.Add(sectionModel);
                }

                courseModel.Status = Aggregate(courseModel.Sections.Select(s => s.Status));
                models.Add(courseModel);
            }

            return models;
        }

        public async Task<IEnumerable<UserCourseModel>> GetUserCoursesAsync(int? userId)
        {
            var user = await ResolveUserAsync(userId);

            if (user is null)
            {
                return Enumerable.Empty<UserCourseModel>();
            }

            var progresses = await _userRepository.GetProgressesAsync(user.Id);

            return progresses.Select(ToModel).ToList();
        }

        public async Task<IEnumerable<LanguageModel>> GetAvailableLanguagesAsync(int? userId)
        {
            var languages = await _languageRepository.GetAllAsync();
            var user = await ResolveUserAsync(userId);

            var enrolled = new HashSet<int>();

            if (user is not null)
            {
                var progresses = await _userRepository.GetProgressesAsync(user.Id);
                enrolled = progresses.Select(lp => lp.LanguageId).ToHashSet();

                // O idioma nativo não é oferecido como curso a ser aprendido.
                enrolled.Add(user.NativeLanguageId);
            }

            return languages
                .Where(l => !enrolled.Contains(l.Id))
                .Select(l => new LanguageModel { Id = l.Id, Code = l.Code, Name = l.Name })
                .ToList();
        }

        public async Task<UserCourseModel> EnrollAsync(int? userId, int languageId)
        {
            var user = await ResolveUserAsync(userId);

            if (user is null)
            {
                throw new InvalidOperationException("Nenhum usuário disponível para matricular no curso.");
            }

            var language = await _languageRepository.GetByIdAsync(languageId);

            if (language is null)
            {
                throw new ArgumentException("Idioma não encontrado.", nameof(languageId));
            }

            var existing = await _userRepository.GetProgressAsync(user.Id, languageId);

            if (existing is null)
            {
                // Toda matrícula nasce zerada: nível 1, sem XP e sem ofensiva.
                await _userRepository.AddProgressAsync(new LanguageProgress
                {
                    UserId = user.Id,
                    LanguageId = languageId,
                    Level = 1,
                    TotalXp = 0,
                    CurrentStreakDays = 0,
                    TotalLearnedWords = 0,
                    TotalCompletedLessons = 0,
                    IsActiveCourse = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // O curso recém-adicionado passa a ser o curso ativo, como no Duolingo.
            await _userRepository.SetActiveCourseAsync(user.Id, languageId);

            var progress = await _userRepository.GetProgressAsync(user.Id, languageId);

            return ToModel(progress);
        }

        public async Task<bool> SetActiveCourseAsync(int? userId, int languageId)
        {
            var user = await ResolveUserAsync(userId);

            if (user is null)
            {
                return false;
            }

            var progress = await _userRepository.GetProgressAsync(user.Id, languageId);

            if (progress is null)
            {
                return false;
            }

            await _userRepository.SetActiveCourseAsync(user.Id, languageId);

            return true;
        }

        public async Task<SectionDetailsModel> GetSectionDetailsAsync(int sectionId, int? userId)
        {
            var section = await _courseRepository.GetSectionDetailsAsync(sectionId);

            if (section is null)
            {
                return null;
            }

            // O status vem da mesma montagem da trilha usada no mapa, para ficar consistente.
            var trackSection = (await GetTrackAsync(userId))
                .SelectMany(c => c.Sections)
                .FirstOrDefault(s => s.Id == sectionId);

            var words = (await _practiceService.GetWordsByLevelAsync(userId, section.Course.LanguageId, section.CefrLevel)).ToList();

            return new SectionDetailsModel
            {
                Id = section.Id,
                CourseId = section.CourseId,
                CourseName = section.Course.Name,
                Title = section.Title,
                Description = section.Description,
                Goal = section.Goal,
                Position = section.Position,
                CefrLevel = section.CefrLevel,
                Status = trackSection?.Status ?? ProgressStatus.Locked,
                TotalUnits = section.Units?.Count ?? 0,
                CompletedUnits = trackSection?.Units.Count(u => u.Status == ProgressStatus.Completed) ?? 0,
                TotalWords = words.Count,
                PracticedWords = words.Count(w => w.SampleAttemptCount > 0),
                MasteredWords = words.Count(w => w.SampleAttemptCount > 0 && w.LearningPercentage >= MasteryThreshold),
                MasteryThreshold = MasteryThreshold,
                Requirements = (section.Requirements ?? new List<SectionRequirement>())
                    .OrderBy(r => r.Position)
                    .Select(r => r.Description)
                    .ToList(),
                GrammarTopics = (section.GrammarTopics ?? new List<GrammarTopic>())
                    .OrderByDescending(g => g.IsMandatory)
                    .ThenBy(g => g.Position)
                    .Select(g => new GrammarTopicModel
                    {
                        Id = g.Id,
                        Position = g.Position,
                        Title = g.Title,
                        Explanation = g.Explanation,
                        Structure = g.Structure,
                        IsMandatory = g.IsMandatory,
                        Examples = (g.Examples ?? string.Empty)
                            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .ToList()
                    })
                    .ToList(),
                Words = words
            };
        }

        private async Task<User> ResolveUserAsync(int? userId)
        {
            return userId.HasValue
                ? await _userRepository.GetWithProgressesAsync(userId.Value)
                : await _userRepository.GetDefaultAsync();
        }

        private static UserCourseModel ToModel(LanguageProgress progress)
        {
            return new UserCourseModel
            {
                LanguageId = progress.LanguageId,
                LanguageCode = progress.Language?.Code,
                LanguageName = progress.Language?.Name,
                Level = progress.Level,
                TotalXp = progress.TotalXp,
                IsActive = progress.IsActiveCourse
            };
        }

        // Consolida o status de um nível da trilha a partir do status dos seus filhos.
        private static ProgressStatus Aggregate(IEnumerable<ProgressStatus> statuses)
        {
            var list = statuses.ToList();

            if (list.Count == 0 || list.All(s => s == ProgressStatus.Locked))
            {
                return ProgressStatus.Locked;
            }

            if (list.All(s => s == ProgressStatus.Completed))
            {
                return ProgressStatus.Completed;
            }

            if (list.Any(s => s is ProgressStatus.Completed or ProgressStatus.InProgress))
            {
                return ProgressStatus.InProgress;
            }

            return ProgressStatus.Available;
        }

        private static ProgressStatus ResolveStatus(
            int lessonId,
            HashSet<int> completed,
            HashSet<int> inProgress,
            ref bool nextLessonAssigned)
        {
            if (completed.Contains(lessonId))
            {
                return ProgressStatus.Completed;
            }

            if (inProgress.Contains(lessonId))
            {
                nextLessonAssigned = true;
                return ProgressStatus.InProgress;
            }

            if (!nextLessonAssigned)
            {
                nextLessonAssigned = true;
                return ProgressStatus.Available;
            }

            return ProgressStatus.Locked;
        }

        private static int? ResolveLanguageId(User user)
        {
            if (user?.LanguageProgresses is null || user.LanguageProgresses.Count == 0)
            {
                return null;
            }

            var active = user.LanguageProgresses.FirstOrDefault(lp => lp.IsActiveCourse)
                ?? user.LanguageProgresses.First();

            return active.LanguageId;
        }
    }
}
