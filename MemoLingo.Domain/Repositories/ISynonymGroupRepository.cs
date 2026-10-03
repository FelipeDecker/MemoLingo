using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de acesso aos grupos de quase-sinônimos e aos seus exercícios de nuance.
    /// </summary>
    public interface ISynonymGroupRepository
    {
        Task<IEnumerable<SynonymGroup>> GetWithExercisesByLanguageAsync(int languageId);
        Task<NuanceExercise> GetExerciseWithGroupAsync(int exerciseId);
        Task<IEnumerable<int>> GetGroupIdsByWordAsync(int wordId);
    }
}
