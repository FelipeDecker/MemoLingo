using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que registra a tentativa de resposta de um exercício, guardando
    /// acerto/erro, tempo de resposta e o momento em que foi respondido.
    /// </summary>
    public class ExerciseAttempt
    {
        public int Id { get; set; }
        public int StudySessionId { get; set; }
        public int? WordId { get; set; }
        public int? SentenceId { get; set; }
        public ExerciseType ExerciseType { get; set; }
        public string GivenAnswer { get; set; }
        public string ExpectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int ResponseTimeMs { get; set; }
        public DateTime AnsweredAt { get; set; }

        public StudySession StudySession { get; set; }
        public Word Word { get; set; }
        public Sentence Sentence { get; set; }
    }
}
