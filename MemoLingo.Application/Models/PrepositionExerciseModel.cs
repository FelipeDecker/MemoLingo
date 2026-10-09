using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class PrepositionExerciseModel
    {
        public int Id { get; set; }
        public PrepositionExerciseType ExerciseType { get; set; }
        public PrepositionUsage Usage { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public string Sentence { get; set; }
        public string Translation { get; set; }
        public int BlankCount { get; set; }
        public bool IsPriorityReview { get; set; }

        public List<string> ShownPrepositions { get; set; }
    }
}
