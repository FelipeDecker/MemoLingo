namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Tópico gramatical estudado em uma seção. Os exemplos são armazenados separados por "|".
    /// </summary>
    public class GrammarTopic
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public string Explanation { get; set; }
        public string Structure { get; set; }
        public string Examples { get; set; }
        public bool IsMandatory { get; set; }

        public Section Section { get; set; }
    }
}
