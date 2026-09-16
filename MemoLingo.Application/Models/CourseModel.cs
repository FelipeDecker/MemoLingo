using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class CourseModel
    {
        public int Id { get; set; }

        public int LanguageId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Position { get; set; }

        public CefrLevel CefrLevel { get; set; }

        public List<LessonModel> Lessons { get; set; } = new();
    }
}
