namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa o desempenho de um usuário em uma palavra específica,
    /// utilizada pelo sistema de repetição espaçada.
    /// </summary>
    public class WordPerformance
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int WordId { get; set; }

        public int StrengthLevel { get; set; }

        public int WrongCount { get; set; }

        public DateTime NextReview { get; set; }
    }
}
