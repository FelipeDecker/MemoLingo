namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Proficiência do usuário em um grupo de quase-sinônimos, usada pela repetição
    /// espaçada da prática de nuances. O grupo é marcado para revisão prioritária
    /// quando o usuário erra uma de suas palavras em qualquer exercício.
    /// </summary>
    public class UserNuanceProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SynonymGroupId { get; set; }
        public int StrengthLevel { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int ProficiencyScore { get; set; }
        public bool IsFlaggedForReview { get; set; }
        public DateTime? LastErrorAt { get; set; }
        public DateTime? LastReview { get; set; }
        public DateTime NextReview { get; set; }

        public SynonymGroup SynonymGroup { get; set; }
        public User User { get; set; }
    }
}
