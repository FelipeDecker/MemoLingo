namespace MemoLingo.Domain.Projections
{
    /// <summary>
    /// Projeção enxuta de uma tentativa de exercício de nuance.
    /// </summary>
    public class NuanceAttemptSample
    {
        public int NuanceExerciseId { get; set; }
        public int SynonymGroupId { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime AnsweredAt { get; set; }
    }
}
