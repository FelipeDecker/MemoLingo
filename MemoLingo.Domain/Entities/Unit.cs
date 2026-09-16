namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma unidade de uma seção, reunindo os nós da
    /// trilha que trabalham um mesmo tópico (ex.: "Saudações").
    /// </summary>
    public class Unit
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Topic { get; set; }
        public string GuidebookMarkdown { get; set; }
        public int Position { get; set; }
        public bool Active { get; set; }

        public Section Section { get; set; }
        public ICollection<PathNode> PathNodes { get; set; }
    }
}
