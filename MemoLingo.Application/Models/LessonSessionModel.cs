using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class LessonSessionModel
    {
        public int StudySessionId { get; set; }
        public int LessonId { get; set; }
        public int PathNodeId { get; set; }
        public int SectionId { get; set; }
        public int UnitId { get; set; }
        public string UnitTitle { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public int LessonPosition { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int XpReward { get; set; }
        public bool IsReview { get; set; }

        public List<LessonExerciseModel> Exercises { get; set; } = new();
    }
}