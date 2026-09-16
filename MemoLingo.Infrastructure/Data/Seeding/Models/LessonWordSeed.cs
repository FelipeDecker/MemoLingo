namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class LessonWordSeed
    {
        public string LanguageCode { get; set; }
        public string CourseName { get; set; }
        public string UnitTitle { get; set; }
        public int NodePosition { get; set; }
        public int LessonPosition { get; set; }
        public string WordText { get; set; }
        public int Position { get; set; }
    }
}
