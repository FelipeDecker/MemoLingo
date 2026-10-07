using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de leitura dos nós da trilha e das lições com o contexto necessário
    /// para montar as atividades (unidade, seção, curso e vocabulário).
    /// </summary>
    public interface ILessonRepository
    {
        Task<PathNode> GetNodeWithContextAsync(int pathNodeId);
        Task<Lesson> GetWithContextAsync(int lessonId);
        Task<IEnumerable<LessonWord>> GetCourseLessonWordsAsync(int courseId);
    }
}
