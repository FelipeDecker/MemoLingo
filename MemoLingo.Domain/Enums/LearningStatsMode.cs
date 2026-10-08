namespace MemoLingo.Domain.Enums
{
    /// <summary>
    /// Base usada para calcular o percentual de aprendizado das palavras.
    /// </summary>
    public enum LearningStatsMode
    {
        // Todos os acertos e erros acumulados em WordPerformance.
        Total = 1,

        // Apenas as últimas tentativas de cada palavra, lidas de ExerciseAttempts (exclusivo Premium).
        RecentAttempts = 2
    }
}
