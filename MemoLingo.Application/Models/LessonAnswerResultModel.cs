namespace MemoLingo.Application.Models
{
    public class LessonAnswerResultModel
    {
        public bool IsCorrect { get; set; }
        public string SubmittedAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public string SentenceText { get; set; }
        public string SentenceTranslation { get; set; }
    }
}