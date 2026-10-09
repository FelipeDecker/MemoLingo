using MemoLingo.Domain.Enums;

namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class PrepositionExerciseSeed
    {
        public string LanguageCode { get; set; }
        public PrepositionExerciseType ExerciseType { get; set; }
        public PrepositionUsage Usage { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public string Sentence { get; set; }
        public string Translation { get; set; }
        public List<string> Answers { get; set; }
        public List<string> ShownPrepositions { get; set; }
        public List<string> AlternativeSentences { get; set; }
        public string Explanation { get; set; }
    }
}
