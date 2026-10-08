namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Requisito que o aluno precisa cumprir para concluir uma seção (nível CEFR).
    /// </summary>
    public class SectionRequirement
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public int Position { get; set; }
        public string Description { get; set; }

        public Section Section { get; set; }
    }
}
