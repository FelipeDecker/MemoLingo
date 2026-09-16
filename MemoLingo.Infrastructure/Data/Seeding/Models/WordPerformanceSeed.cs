namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class WordPerformanceSeed
    {
        public string UserEmail { get; set; }
        public string LanguageCode { get; set; }
        public string WordText { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int StrengthLevel { get; set; }
    }
}
