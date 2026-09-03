namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade de associação entre Lesson e Word, indicando quais palavras
    /// são trabalhadas nos exercícios de uma lição.
    /// </summary>
    public class LessonWord
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public int WordId { get; set; }
        public int Position { get; set; }

        public Lesson Lesson { get; set; }
        public Word Word { get; set; }
    }
}
