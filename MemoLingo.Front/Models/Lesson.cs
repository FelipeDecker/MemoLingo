namespace MemoLingo.Front.Models
{
    /// <summary>
    /// Representa uma lição (bolinha) dentro de uma trilha de aprendizado.
    /// </summary>
    public class Lesson
    {
        public int Id { get; set; }

        /// <summary>
        /// Id da unidade à qual esta lição pertence, usado para agrupar as lições por unidade.
        /// </summary>
        public int UnitId { get; set; }

        public string Title { get; set; } = string.Empty;

        public LessonType Type { get; set; }

        public LessonStatus Status { get; set; }

        /// <summary>
        /// Ordem de exibição da lição dentro da unidade (define a posição na trilha em zigue-zague).
        /// </summary>
        public int Order { get; set; }
    }
}
