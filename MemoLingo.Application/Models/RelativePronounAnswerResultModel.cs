namespace MemoLingo.Application.Models
{
    public class RelativePronounAnswerResultModel
    {
        public int ExerciseId { get; set; }
        public bool IsCorrect { get; set; }
        public bool PronounsCorrect { get; set; }
        public string Sentence { get; set; }
        public string CorrectSentence { get; set; }
        public string SubmittedSentence { get; set; }
        public string Translation { get; set; }
        public int? MistakeIndex { get; set; }
        public int? SubmittedMistakeIndex { get; set; }
        public string Explanation { get; set; }

        public List<string> CorrectAnswers { get; set; }
        public List<RelativePronounBlankResultModel> Blanks { get; set; }
    }
}
