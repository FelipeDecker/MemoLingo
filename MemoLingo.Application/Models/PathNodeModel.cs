using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class PathNodeModel
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public NodeType NodeType { get; set; }
        public int Position { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public ProgressStatus Status { get; set; }

        public List<LessonModel> Lessons { get; set; } = new();
    }
}
