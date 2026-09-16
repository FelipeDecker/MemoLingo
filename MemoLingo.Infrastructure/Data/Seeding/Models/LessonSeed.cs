using MemoLingo.Domain.Enums;

namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class LessonSeed
    {
        public string LanguageCode { get; set; }
        public string CourseName { get; set; }
        public string Title { get; set; }
        public string Topic { get; set; }
        public int Position { get; set; }
        public int ExerciseCount { get; set; }
        public int XpReward { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public bool Active { get; set; }
    }
}
