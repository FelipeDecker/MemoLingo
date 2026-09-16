using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma seção de um curso, agrupando unidades de um
    /// mesmo estágio de proficiência (ex.: "Seção 1 - Fundamentos").
    /// </summary>
    public class Section
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Position { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public bool Active { get; set; }

        public Course Course { get; set; }
        public ICollection<Unit> Units { get; set; }
    }
}
