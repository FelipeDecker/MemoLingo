namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade de associação entre Sentence e Word, indicando quais palavras
    /// compõem uma frase e em que posição aparecem.
    /// </summary>
    public class SentenceWord
    {
        public int Id { get; set; }
        public int SentenceId { get; set; }
        public int WordId { get; set; }
        public int Position { get; set; }

        public Sentence Sentence { get; set; }
        public Word Word { get; set; }
    }
}
