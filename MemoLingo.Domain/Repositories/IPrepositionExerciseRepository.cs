using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de acesso aos exercícios da coleção de preposições.
    /// </summary>
    public interface IPrepositionExerciseRepository
    {
        Task<IEnumerable<PrepositionExercise>> GetByLanguageAsync(int languageId, PrepositionExerciseType? exerciseType);
        Task<PrepositionExercise> GetByIdAsync(int id);
    }
}
