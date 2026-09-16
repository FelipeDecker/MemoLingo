using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um exercício individual de uma lição, com o
    /// enunciado, a resposta esperada e as alternativas quando aplicável.
    /// </summary>
    public class Challenge
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public int? WordId { get; set; }
        public int? SentenceId { get; set; }
        public ExerciseType ExerciseType { get; set; }
        public string Prompt { get; set; }
        public string ExpectedAnswer { get; set; }
        public string OptionsJson { get; set; }
        public int Position { get; set; }
        public bool Active { get; set; }

        public Lesson Lesson { get; set; }
        public Word Word { get; set; }
        public Sentence Sentence { get; set; }
        public ICollection<ExerciseAttempt> ExerciseAttempts { get; set; }
    }
}
