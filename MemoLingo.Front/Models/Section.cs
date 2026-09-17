using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Models
{
    /// <summary>
    /// Representa uma seção do curso: um agrupamento de unidades que compartilham
    /// o mesmo nível de proficiência (CEFR).
    /// </summary>
    public class Section
    {
        public int Id { get; set; }

        /// <summary>
        /// Número sequencial exibido na interface (ex.: "Seção 4").
        /// </summary>
        public int Number { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Nível CEFR da seção (ex.: "A1", "B2").
        /// </summary>
        public string CefrLevel { get; set; }

        /// <summary>
        /// Cor principal usada nos elementos visuais da seção.
        /// </summary>
        public string PrimaryColor { get; set; }

        public ProgressStatus Status { get; set; }

        /// <summary>
        /// Percentual de lições concluídas na seção (0 a 100).
        /// </summary>
        public int ProgressPercent { get; set; }

        public List<Unit> Units { get; set; } = new();
    }
}
