namespace MemoLingo.Domain.Projections
{
    /// <summary>
    /// Projeção enxuta de uma tentativa de exercício de pronome relativo.
    /// </summary>
    public class RelativePronounAttemptSample
    {
        public int RelativePronounExerciseId { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime AnsweredAt { get; set; }
    }
}
