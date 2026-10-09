using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de acesso aos exercícios da coleção de pronomes relativos.
    /// </summary>
    public interface IRelativePronounExerciseRepository
    {
        Task<IEnumerable<RelativePronounExercise>> GetByLanguageAsync(int languageId, RelativePronounExerciseType? exerciseType);
        Task<RelativePronounExercise> GetByIdAsync(int id);
    }
}
