namespace MemoLingo.Application.Models
{
    public class NuanceGroupProgressModel
    {
        public int SynonymGroupId { get; set; }
        public int ProficiencyScore { get; set; }
        public int StrengthLevel { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public bool IsFlaggedForReview { get; set; }
        public DateTime NextReview { get; set; }
    }
}
