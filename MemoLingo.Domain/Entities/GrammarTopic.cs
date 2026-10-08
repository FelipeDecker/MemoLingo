namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Tópico gramatical estudado em uma seção. Os exemplos são armazenados separados por "|".
    /// Os marcadores são expressões regulares (uma por linha) que identificam as frases que
    /// exercitam o tópico; são usados para montar o teste de salto de seção.
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
        public string Markers { get; set; }
        public bool IsMandatory { get; set; }

        public Section Section { get; set; }
    }
}
