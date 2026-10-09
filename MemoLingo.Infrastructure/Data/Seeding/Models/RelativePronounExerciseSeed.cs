using MemoLingo.Domain.Enums;

namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class RelativePronounExerciseSeed
    {
        public string LanguageCode { get; set; }
        public RelativePronounExerciseType ExerciseType { get; set; }
        public RelativePronounUsage Usage { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public string Sentence { get; set; }
        public string Translation { get; set; }
        public List<string> Answers { get; set; }
        public List<string> ShownPronouns { get; set; }
        public List<string> AlternativeSentences { get; set; }
        public string Explanation { get; set; }
    }
}
