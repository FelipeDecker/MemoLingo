using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de leitura das trilhas/cursos com sua hierarquia completa
    /// (seções, unidades, nós e lições).
    /// </summary>
    public interface ICourseRepository
    {
        Task<IEnumerable<Course>> GetActiveTrackAsync(int? languageId);
    }
}
