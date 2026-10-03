using MemoLingo.Application.Models;

namespace MemoLingo.Application.Services
{
    public interface INuanceService
    {
        Task<IEnumerable<NuanceExerciseModel>> GetSessionAsync(int userId, int take = 10);
        Task<NuanceAnswerResultModel> SubmitAnswerAsync(NuanceAnswerModel answer);
        Task FlagGroupsForWordErrorAsync(int userId, int wordId);
    }
}
