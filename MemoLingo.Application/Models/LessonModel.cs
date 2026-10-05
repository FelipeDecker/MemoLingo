using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class LessonModel
    {
        public int Id { get; set; }
        public int PathNodeId { get; set; }
        public int Position { get; set; }
        public int XpReward { get; set; }
        public ProgressStatus Status { get; set; }
    }
}
