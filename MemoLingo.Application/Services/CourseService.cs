using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IStudySessionRepository _studySessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILanguageRepository _languageRepository;

        public CourseService(
            ICourseRepository courseRepository,
            IStudySessionRepository studySessionRepository,
            IUserRepository userRepository,
            ILanguageRepository languageRepository)
        {
            _courseRepository = courseRepository;
            _studySessionRepository = studySessionRepository;
            _userRepository = userRepository;
            _languageRepository = languageRepository;
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

            foreach (var course in courses.OrderBy(c => c.Position))
            {
                var model = new CourseModel
                {
                    Id = course.Id,
                    LanguageId = course.LanguageId,
                    Name = course.Name,
                    Description = course.Description,
                    Position = course.Position,
                    CefrLevel = course.CefrLevel
                };

                var flattened = Flatten(course).ToList();

                for (var index = 0; index < flattened.Count; index++)
                {
                    var item = flattened[index];
                    var status = ResolveStatus(item.Lesson.Id, completed, inProgress, ref nextLessonAssigned);

                    model.Lessons.Add(new LessonModel
                    {
                        Id = item.Lesson.Id,
                        CourseId = course.Id,
                        SectionId = item.Section.Id,
                        UnitId = item.Unit.Id,
                        PathNodeId = item.Node.Id,
                        NodeType = item.Node.NodeType,
                        Title = item.Unit.Title,
                        Topic = item.Unit.Topic,
                        Position = index + 1,
                        XpReward = item.Lesson.XpReward,
                        CefrLevel = item.Section.CefrLevel,
                        Status = status,
                        IsLast = index == flattened.Count - 1
                    });
                }

                models.Add(model);
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

        private static IEnumerable<(Section Section, Unit Unit, PathNode Node, Lesson Lesson)> Flatten(Course course)
        {
            foreach (var section in (course.Sections ?? new List<Section>()).OrderBy(s => s.Position))
            {
                foreach (var unit in (section.Units ?? new List<Unit>()).OrderBy(u => u.Position))
                {
                    foreach (var node in (unit.PathNodes ?? new List<PathNode>()).OrderBy(pn => pn.Position))
                    {
                        foreach (var lesson in (node.Lessons ?? new List<Lesson>()).OrderBy(l => l.Position))
                        {
                            yield return (section, unit, node, lesson);
                        }
                    }
                }
            }
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
