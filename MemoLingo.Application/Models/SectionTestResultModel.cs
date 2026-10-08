namespace MemoLingo.Application.Models
{
    public class SectionTestResultModel
    {
        public int StudySessionId { get; set; }
        public int SectionId { get; set; }
        public string SectionTitle { get; set; }
        public bool Passed { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int TotalExercises { get; set; }
        public int RequiredCorrect { get; set; }
        public int AccuracyPercentage { get; set; }
        public int SkippedLessons { get; set; }
    }
}
