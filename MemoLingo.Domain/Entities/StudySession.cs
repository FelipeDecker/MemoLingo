using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma sessão de estudo, agrupando todos os
    /// exercícios respondidos pelo usuário em uma única prática.
    /// Sessões de lição têm LessonId; testes de salto de seção têm SectionId (seção que será
    /// liberada) e ExerciseCount (quantidade de questões sorteadas); a prática livre não tem nenhum dos dois.
    /// </summary>
    public class StudySession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LanguageId { get; set; }
        public int? LessonId { get; set; }
        public int? SectionId { get; set; }
        public int? ExerciseCount { get; set; }
        public ProgressStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int XpEarned { get; set; }

        public User User { get; set; }
        public Language Language { get; set; }
        public Lesson Lesson { get; set; }
        public Section Section { get; set; }
        public ICollection<ExerciseAttempt> ExerciseAttempts { get; set; }
    }
}
