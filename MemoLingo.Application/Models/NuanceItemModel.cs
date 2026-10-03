namespace MemoLingo.Application.Models
{
    public class NuanceItemModel
    {
        public int WordId { get; set; }
        public string Text { get; set; }
        public string Translation { get; set; }
        public string NuanceExplanation { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public bool IsSubmittedAnswer { get; set; }
    }
}
