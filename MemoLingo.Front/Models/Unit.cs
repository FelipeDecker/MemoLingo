namespace MemoLingo.Front.Models
{
    /// <summary>
    /// Representa uma unidade (seção) que agrupa várias lições em uma trilha.
    /// </summary>
    public class Unit
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Cor principal usada no banner da unidade e nas bolinhas concluídas/disponíveis.
        /// </summary>
        public string PrimaryColor { get; set; } = "#58cc02";

        public List<Lesson> Lessons { get; set; } = new();
    }
}
