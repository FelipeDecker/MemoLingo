using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma lição, ou seja, um conjunto de exercícios
    /// sobre um tópico dentro de uma trilha/curso.
    /// </summary>
    public class Lesson
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Topic { get; set; }
        public int Position { get; set; }
        public int ExerciseCount { get; set; }
        public int XpReward { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public bool Active { get; set; }

        public Course Course { get; set; }
        public ICollection<LessonWord> LessonWords { get; set; }
        public ICollection<StudySession> StudySessions { get; set; }
    }
}
