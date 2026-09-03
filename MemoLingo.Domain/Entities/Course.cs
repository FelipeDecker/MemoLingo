using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma trilha/curso de um idioma, composta por uma
    /// sequência ordenada de lições (ex.: "Básico", "Viagem").
    /// </summary>
    public class Course
    {
        public int Id { get; set; }
        public int LanguageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Position { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public bool Active { get; set; }

        public Language Language { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
    }
}
