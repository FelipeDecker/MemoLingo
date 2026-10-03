namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class NuanceExerciseSeed
    {
        public string LanguageCode { get; set; }
        public string GroupName { get; set; }
        public string TargetWordText { get; set; }
        public string SentenceContext { get; set; }
        public string SentenceTranslation { get; set; }
        public List<string> AcceptedAnswers { get; set; }
        public string Explanation { get; set; }
    }
}
