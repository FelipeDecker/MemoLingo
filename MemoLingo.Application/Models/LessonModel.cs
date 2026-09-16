using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class LessonModel
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public int SectionId { get; set; }

        public int UnitId { get; set; }

        public int PathNodeId { get; set; }

        public NodeType NodeType { get; set; }

        public string Title { get; set; }

        public string Topic { get; set; }

        public int Position { get; set; }

        public int XpReward { get; set; }

        public CefrLevel CefrLevel { get; set; }

        public ProgressStatus Status { get; set; }

        public bool IsLast { get; set; }
    }
}
