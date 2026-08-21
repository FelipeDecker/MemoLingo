namespace MemoLingo.Front.Models
{
    /// <summary>
    /// Representa uma unidade (seção) que agrupa várias lições em uma trilha.
    /// </summary>
    public class Unidade
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Cor principal usada no banner da unidade e nas bolinhas concluídas/disponíveis.
        /// </summary>
        public string CorPrimaria { get; set; } = "#58cc02";

        public List<Licao> Licoes { get; set; } = new();
    }
}
