using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class SectionDetailsModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Goal { get; set; }
        public int Position { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public ProgressStatus Status { get; set; }
        public int TotalUnits { get; set; }
        public int CompletedUnits { get; set; }
        public int TotalWords { get; set; }
        public int PracticedWords { get; set; }
        public int MasteredWords { get; set; }
        public int MasteryThreshold { get; set; }

        public List<string> Requirements { get; set; } = new();
        public List<GrammarTopicModel> GrammarTopics { get; set; } = new();
        public List<PracticeWordModel> Words { get; set; } = new();
    }
}
