using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class RelativePronounExerciseModel
    {
        public int Id { get; set; }
        public RelativePronounExerciseType ExerciseType { get; set; }
        public RelativePronounUsage Usage { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public string Sentence { get; set; }
        public string Translation { get; set; }
        public int BlankCount { get; set; }
        public bool IsPriorityReview { get; set; }

        public List<string> ShownPronouns { get; set; }
    }
}
