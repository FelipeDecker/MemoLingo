namespace MemoLingo.Application.Models
{
    public class NuanceAnswerModel
    {
        public int UserId { get; set; }
        public int ExerciseId { get; set; }
        public string SubmittedAnswer { get; set; }
        public int? SubmittedWordId { get; set; }
    }
}
