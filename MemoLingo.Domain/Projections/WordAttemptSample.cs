namespace MemoLingo.Domain.Projections
{
    /// <summary>
    /// Projeção enxuta de uma tentativa de exercício de uma palavra, usada para
    /// calcular estatísticas sobre a janela das tentativas mais recentes.
    /// </summary>
    public class WordAttemptSample
    {
        public int WordId { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime AnsweredAt { get; set; }
    }
}
