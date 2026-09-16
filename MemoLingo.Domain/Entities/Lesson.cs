namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma lição, ou seja, um conjunto de exercícios
    /// (challenges) executados a partir de um nó da trilha.
    /// </summary>
    public class Lesson
    {
        public int Id { get; set; }
        public int PathNodeId { get; set; }
        public int Position { get; set; }
        public int XpReward { get; set; }
        public bool Active { get; set; }

        public PathNode PathNode { get; set; }
        public ICollection<Challenge> Challenges { get; set; }
        public ICollection<LessonWord> LessonWords { get; set; }
        public ICollection<StudySession> StudySessions { get; set; }
    }
}
