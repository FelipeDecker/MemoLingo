using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class SectionTestSessionModel
    {
        public int StudySessionId { get; set; }
        public int SectionId { get; set; }
        public string SectionTitle { get; set; }
        public int SourceSectionId { get; set; }
        public string SourceSectionTitle { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public int TotalExercises { get; set; }
        public int RequiredCorrect { get; set; }
        public int MaxWrong { get; set; }
        public int PassPercentage { get; set; }

        public List<string> GrammarTopics { get; set; } = new();
        public List<LessonExerciseModel> Exercises { get; set; } = new();
    }
}
