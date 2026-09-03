using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma sessão de estudo, agrupando todos os
    /// exercícios respondidos pelo usuário em uma única prática.
    /// </summary>
    public class StudySession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LanguageId { get; set; }
        public int? LessonId { get; set; }
        public ProgressStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int XpEarned { get; set; }

        public User User { get; set; }
        public Language Language { get; set; }
        public Lesson Lesson { get; set; }
        public ICollection<ExerciseAttempt> ExerciseAttempts { get; set; }
    }
}
