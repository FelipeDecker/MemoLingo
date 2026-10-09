namespace MemoLingo.Domain.Projections
{
    /// <summary>
    /// Projeção enxuta de uma tentativa de exercício de preposição.
    /// </summary>
    public class PrepositionAttemptSample
    {
        public int PrepositionExerciseId { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime AnsweredAt { get; set; }
    }
}
