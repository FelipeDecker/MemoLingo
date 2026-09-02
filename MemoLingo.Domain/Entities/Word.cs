namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma palavra a ser aprendida, pertencente a um idioma.
    /// </summary>
    public class Word
    {
        public int Id { get; set; }

        public int LanguageId { get; set; }

        public Language Language { get; set; }

        public string Text { get; set; }

        public string Translation { get; set; }
    }
}
