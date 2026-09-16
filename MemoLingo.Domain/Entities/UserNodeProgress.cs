namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que rastreia quantas lições o usuário já concluiu dentro de um
    /// nó específico da trilha.
    /// </summary>
    public class UserNodeProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PathNodeId { get; set; }
        public int CompletedLessonsCount { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? LastPracticedAt { get; set; }

        public User User { get; set; }
        public PathNode PathNode { get; set; }
    }
}
