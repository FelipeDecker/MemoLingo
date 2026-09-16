using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class LessonModel
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public string Title { get; set; }

        public string Topic { get; set; }

        public int Position { get; set; }

        public int ExerciseCount { get; set; }

        public int XpReward { get; set; }

        public CefrLevel CefrLevel { get; set; }

        public ProgressStatus Status { get; set; }

        public bool IsLast { get; set; }
    }
}
