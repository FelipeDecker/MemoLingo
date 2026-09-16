using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    public class LessonService : ILessonService
    {
        // Paleta usada para diferenciar visualmente cada unidade da trilha.
        private static readonly string[] UnitColors = { "#58cc02", "#1cb0f6", "#ce82ff", "#ff9600", "#ff4b4b" };

        private readonly ICoursesClient _coursesClient;

        public LessonService(ICoursesClient coursesClient)
        {
            _coursesClient = coursesClient;
        }

        public async Task<List<Unit>> GetUnitsAsync()
        {
            var courses = await _coursesClient.GetAsync(null);

            return courses
                .OrderBy(c => c.Position)
                .Select(ToUnit)
                .ToList();
        }

        private static Unit ToUnit(CourseModel course, int index)
        {
            var unit = new Unit
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                PrimaryColor = UnitColors[index % UnitColors.Length]
            };

            var lessons = course.Lessons ?? new List<LessonModel>();

            foreach (var lesson in lessons.OrderBy(l => l.Position))
            {
                unit.Lessons.Add(new Lesson
                {
                    Id = lesson.Id,
                    UnitId = course.Id,
                    Title = lesson.Title,
                    Topic = lesson.Topic,
                    Type = lesson.IsLast ? LessonType.Exam : LessonType.Lesson,
                    Status = lesson.Status,
                    Order = lesson.Position
                });
            }

            return unit;
        }
    }
}
