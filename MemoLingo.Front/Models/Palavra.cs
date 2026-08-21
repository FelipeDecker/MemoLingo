namespace MemoLingo.Front.Models
{
    /// <summary>
    /// Representa uma palavra/expressão do vocabulário do usuário, junto com seu
    /// histórico de acertos e erros. É a base do modo de prática focado em reforçar
    /// justamente as palavras em que o usuário mais erra.
    /// </summary>
    public class Palavra
    {
        public int Id { get; set; }

        /// <summary>
        /// Palavra ou expressão no idioma que está sendo aprendido.
        /// </summary>
        public string Texto { get; set; } = string.Empty;

        /// <summary>
        /// Tradução da palavra, exibida quando o usuário revela a resposta.
        /// </summary>
        public string Traducao { get; set; } = string.Empty;

        public int QuantidadeErros { get; set; }

        public int QuantidadeAcertos { get; set; }

        /// <summary>
        /// Taxa de acerto (0 a 1) usada apenas para exibição; palavras nunca
        /// respondidas são tratadas como taxa 0 (prioridade máxima de prática).
        /// </summary>
        public double TaxaAcerto
        {
            get
            {
                var total = QuantidadeAcertos + QuantidadeErros;
                return total == 0 ? 0 : (double)QuantidadeAcertos / total;
            }
        }
    }
}
