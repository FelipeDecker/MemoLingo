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
            var sections = await GetSectionsAsync();

            return sections.SelectMany(s => s.Units).ToList();
        }

        public async Task<List<Section>> GetSectionsAsync()
        {
            var courses = await _coursesClient.GetAsync(null);

            var sections = new List<Section>();
            var number = 0;

            // A API entrega os cursos já ordenados por nível; cada nível CEFR vira uma seção
            // e os cursos daquele nível viram as unidades da trilha.
            var groups = courses
                .OrderBy(c => c.CefrLevel)
                .ThenBy(c => c.Position)
                .GroupBy(c => c.CefrLevel);

            foreach (var group in groups)
            {
                number++;

                var section = new Section
                {
                    Id = number,
                    Number = number,
                    Title = $"Seção {number}",
                    Description = group.First().Description,
                    CefrLevel = group.Key.ToString(),
                    PrimaryColor = UnitColors[(number - 1) % UnitColors.Length]
                };

                var index = 0;

                foreach (var course in group)
                {
                    section.Units.Add(ToUnit(course, index));
                    index++;
                }

                ApplyProgress(section);
                sections.Add(section);
            }

            return sections;
        }

        public async Task<Section> GetSectionAsync(int sectionId)
        {
            var sections = await GetSectionsAsync();

            return sections.FirstOrDefault(s => s.Id == sectionId) ?? sections.FirstOrDefault();
        }

        public async Task<Section> GetCurrentSectionAsync()
        {
            var sections = await GetSectionsAsync();

            return sections.FirstOrDefault(s => s.Status == ProgressStatus.InProgress)
                ?? sections.FirstOrDefault(s => s.Status != ProgressStatus.Completed)
                ?? sections.FirstOrDefault();
        }

        /// <summary>
        /// Calcula o percentual de conclusão e o status da seção a partir das lições das suas unidades.
        /// </summary>
        private static void ApplyProgress(Section section)
        {
            var lessons = section.Units.SelectMany(u => u.Lessons).ToList();

            if (lessons.Count == 0)
            {
                section.Status = ProgressStatus.Locked;
                return;
            }

            var completed = lessons.Count(l => l.Status == ProgressStatus.Completed);

            section.ProgressPercent = (int)Math.Round(completed * 100d / lessons.Count);

            if (completed == lessons.Count)
            {
                section.Status = ProgressStatus.Completed;
            }
            else if (lessons.Any(l => l.Status is ProgressStatus.Available or ProgressStatus.InProgress or ProgressStatus.Completed))
            {
                section.Status = ProgressStatus.InProgress;
            }
            else
            {
                section.Status = ProgressStatus.Locked;
            }
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
