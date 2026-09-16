using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um nó da trilha (a "bolinha" do mapa) dentro de
    /// uma unidade, agrupando as lições necessárias para concluí-lo.
    /// </summary>
    public class PathNode
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public NodeType NodeType { get; set; }
        public int Position { get; set; }
        public int TotalLessons { get; set; }
        public bool Active { get; set; }

        public Unit Unit { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
        public ICollection<UserNodeProgress> UserNodeProgresses { get; set; }
    }
}
