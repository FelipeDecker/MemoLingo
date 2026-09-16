using MemoLingo.Domain.Enums;

namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class LessonProgressSeed
    {
        public string UserEmail { get; set; }
        public string LanguageCode { get; set; }
        public string CourseName { get; set; }
        public string LessonTitle { get; set; }
        public ProgressStatus Status { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int XpEarned { get; set; }
    }
}
