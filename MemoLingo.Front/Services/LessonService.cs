using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    public class LessonService : ILessonService
    {
        public async Task<List<Unit>> GetUnitsAsync()
        {
            // Simula a latência de uma chamada real a uma API.
            await Task.Delay(300);

            return GetMockUnits();
        }

        private static List<Unit> GetMockUnits()
        {
            var units = new List<Unit>
            {
                new Unit
                {
                    Id = 1,
                    Name = "Unidade 1",
                    Description = "Frases básicas do dia a dia",
                    PrimaryColor = "#58cc02"
                },
                new Unit
                {
                    Id = 2,
                    Name = "Unidade 2",
                    Description = "Gratidão: agradeça pela ajuda",
                    PrimaryColor = "#1cb0f6"
                },
                new Unit
                {
                    Id = 3,
                    Name = "Unidade 3",
                    Description = "Comidas e bebidas",
                    PrimaryColor = "#ce82ff"
                }
            };

            // Definição de quantas lições cada unidade tem e como elas se dividem por tipo.
            var lessonsByUnit = new Dictionary<int, List<(LessonType Type, LessonStatus Status)>>
            {
                [1] = new()
                {
                    (LessonType.Lesson, LessonStatus.Completed),
                    (LessonType.Lesson, LessonStatus.Completed),
                    (LessonType.Story, LessonStatus.Completed),
                    (LessonType.Lesson, LessonStatus.Completed),
                    (LessonType.Chest, LessonStatus.Completed),
                    (LessonType.Exam, LessonStatus.Completed)
                },
                [2] = new()
                {
                    (LessonType.Lesson, LessonStatus.Completed),
                    (LessonType.Story, LessonStatus.Completed),
                    (LessonType.Lesson, LessonStatus.Completed),
                    (LessonType.Chest, LessonStatus.Available),
                    (LessonType.Lesson, LessonStatus.Available),
                    (LessonType.Lesson, LessonStatus.Locked),
                    (LessonType.Exam, LessonStatus.Locked)
                },
                [3] = new()
                {
                    (LessonType.Lesson, LessonStatus.Locked),
                    (LessonType.Lesson, LessonStatus.Locked),
                    (LessonType.Story, LessonStatus.Locked),
                    (LessonType.Chest, LessonStatus.Locked),
                    (LessonType.Exam, LessonStatus.Locked)
                }
            };

            var lessonId = 1;

            // Foreach que "publica" cada lição mockada dentro da unidade correspondente,
            // preenchendo o UnitId e a ordem de exibição na trilha.
            foreach (var unit in units)
            {
                var order = 1;

                foreach (var (type, status) in lessonsByUnit[unit.Id])
                {
                    unit.Lessons.Add(new Lesson
                    {
                        Id = lessonId++,
                        UnitId = unit.Id,
                        Title = $"{unit.Name} - Lição {order}",
                        Type = type,
                        Status = status,
                        Order = order
                    });

                    order++;
                }
            }

            return units;
        }
    }
}
