namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade de associação entre User e Language, representando o progresso
    /// de um usuário em um idioma que está aprendendo.
    /// </summary>
    public class LanguageProgress
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int LanguageId { get; set; }

        public Language Language { get; set; }

        public int Level { get; set; }

        public int TotalXp { get; set; }

        public bool IsActiveCourse { get; set; }

        public int TotalLearnedWords { get; set; }

        public int TotalCompletedLessons { get; set; }

        public int CurrentStreakDays { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
