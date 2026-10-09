namespace MemoLingo.Application.Models
{
    public class RelativePronounAnswerModel
    {
        public int UserId { get; set; }
        public int ExerciseId { get; set; }
        public string SubmittedSentence { get; set; }
        public int? MistakeIndex { get; set; }
        public string Correction { get; set; }

        public List<string> Answers { get; set; }
    }
}
