using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma frase de exemplo em um idioma, com sua tradução
    /// e as palavras associadas utilizadas nos exercícios.
    /// </summary>
    public class Sentence
    {
        public int Id { get; set; }
        public int LanguageId { get; set; }
        public string Text { get; set; }
        public string Translation { get; set; }
        public CefrLevel CefrLevel { get; set; }

        public Language Language { get; set; }
        public ICollection<SentenceWord> SentenceWords { get; set; }
    }
}
