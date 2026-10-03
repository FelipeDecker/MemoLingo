using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public interface INuanceService
    {
        Task<List<NuanceExerciseModel>> GetSessionAsync();
        Task<NuanceAnswerResultModel> SubmitAnswerAsync(int exerciseId, string answer, int? wordId);
    }
}
