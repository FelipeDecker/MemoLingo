using MemoLingo.Application.Models;
using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Services
{
    public interface IPrepositionService
    {
        Task<IEnumerable<PrepositionExerciseModel>> GetSessionAsync(int userId, int take, PrepositionExerciseType? exerciseType);
        Task<PrepositionAnswerResultModel> SubmitAnswerAsync(PrepositionAnswerModel answer);
    }
}
