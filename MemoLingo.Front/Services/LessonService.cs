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

            // A API entrega a cadeia completa (curso > seção > unidade > nó > lição):
            // cada seção vira uma tela da trilha e cada nó vira uma bolinha da sua unidade.
            var apiSections = courses
                .OrderBy(c => c.Position)
                .SelectMany(c => (c.Sections ?? new List<SectionModel>()).OrderBy(s => s.Position));

            foreach (var apiSection in apiSections)
            {
                number++;

                var section = new Section
                {
                    Id = apiSection.Id,
                    Number = number,
                    Title = apiSection.Title,
                    Description = apiSection.Description,
                    CefrLevel = apiSection.CefrLevel.ToString(),
                    PrimaryColor = UnitColors[(number - 1) % UnitColors.Length],
                    Status = apiSection.Status
                };

                var index = 0;

                foreach (var unit in (apiSection.Units ?? new List<UnitModel>()).OrderBy(u => u.Position))
                {
                    section.Units.Add(ToUnit(unit, index));
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

            return sections.FirstOrDefault(s => s.Status is ProgressStatus.InProgress or ProgressStatus.Available)
                ?? sections.FirstOrDefault(s => s.Status != ProgressStatus.Completed)
                ?? sections.FirstOrDefault();
        }

        /// <summary>
        /// Calcula o percentual de conclusão da seção a partir dos nós (bolinhas) das suas unidades.
        /// </summary>
        private static void ApplyProgress(Section section)
        {
            var lessons = section.Units.SelectMany(u => u.Lessons).ToList();

            if (lessons.Count == 0)
            {
                section.ProgressPercent = 0;
                return;
            }

            var completed = lessons.Count(l => l.Status == ProgressStatus.Completed);

            section.ProgressPercent = (int)Math.Round(completed * 100d / lessons.Count);
        }

        private static Unit ToUnit(UnitModel apiUnit, int index)
        {
            var unit = new Unit
            {
                Id = apiUnit.Id,
                Name = apiUnit.Title,
                Description = apiUnit.Topic,
                PrimaryColor = UnitColors[index % UnitColors.Length]
            };

            var nodes = (apiUnit.PathNodes ?? new List<PathNodeModel>()).OrderBy(pn => pn.Position).ToList();

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                unit.Lessons.Add(new Lesson
                {
                    Id = node.Id,
                    UnitId = apiUnit.Id,
                    Title = apiUnit.Title,
                    Topic = apiUnit.Topic,
                    Type = i == nodes.Count - 1 ? LessonType.Exam : LessonType.Lesson,
                    Status = node.Status,
                    Order = node.Position
                });
            }

            return unit;
        }
    }
}
