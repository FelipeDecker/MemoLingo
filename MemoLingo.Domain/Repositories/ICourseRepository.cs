using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de leitura das trilhas/cursos com suas lições.
    /// </summary>
    public interface ICourseRepository
    {
        Task<IEnumerable<Course>> GetActiveWithLessonsAsync(int? languageId);
    }
}
