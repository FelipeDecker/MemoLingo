namespace MemoLingo.Front.Models
{
    /// <summary>
    /// Representa uma palavra/expressão do vocabulário do usuário, junto com seu
    /// histórico de acertos e erros. É a base do modo de prática focado em reforçar
    /// justamente as palavras em que o usuário mais erra.
    /// </summary>
    public class Word
    {
        public int Id { get; set; }

        /// <summary>
        /// Palavra ou expressão no idioma que está sendo aprendido.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Tradução da palavra, exibida quando o usuário revela a resposta.
        /// </summary>
        public string Translation { get; set; } = string.Empty;

        public int WrongCount { get; set; }

        public int CorrectCount { get; set; }

        /// <summary>
        /// Taxa de acerto (0 a 1) usada apenas para exibição; palavras nunca
        /// respondidas são tratadas como taxa 0 (prioridade máxima de prática).
        /// </summary>
        public double AccuracyRate
        {
            get
            {
                var total = CorrectCount + WrongCount;
                return total == 0 ? 0 : (double)CorrectCount / total;
            }
        }
    }
}
