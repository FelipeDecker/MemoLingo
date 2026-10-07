namespace MemoLingo.Application.Models
{
    public class LessonCompletionModel
    {
        public int StudySessionId { get; set; }
        public int LessonId { get; set; }
        public int PathNodeId { get; set; }
        public int XpEarned { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int AccuracyPercentage { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public bool NodeCompleted { get; set; }
    }
}