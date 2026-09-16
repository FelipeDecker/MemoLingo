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

        public CourseService(
            ICourseRepository courseRepository,
            IStudySessionRepository studySessionRepository,
            IUserRepository userRepository)
        {
            _courseRepository = courseRepository;
            _studySessionRepository = studySessionRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<CourseModel>> GetTrackAsync(int? userId)
        {
            var user = userId.HasValue
                ? await _userRepository.GetWithProgressesAsync(userId.Value)
                : await _userRepository.GetDefaultAsync();

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
