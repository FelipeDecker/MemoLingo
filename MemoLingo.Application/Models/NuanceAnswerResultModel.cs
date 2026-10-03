namespace MemoLingo.Application.Models
{
    public class NuanceAnswerResultModel
    {
        public int ExerciseId { get; set; }
        public bool IsCorrect { get; set; }
        public string SubmittedAnswer { get; set; }
        public int? SubmittedWordId { get; set; }
        public int CorrectWordId { get; set; }
        public string CorrectWord { get; set; }
        public string CorrectAnswer { get; set; }
        public string CompletedSentence { get; set; }
        public string SentenceTranslation { get; set; }
        public string Explanation { get; set; }

        public List<NuanceItemModel> Nuances { get; set; }
        public NuanceGroupProgressModel GroupProgress { get; set; }
    }
}
