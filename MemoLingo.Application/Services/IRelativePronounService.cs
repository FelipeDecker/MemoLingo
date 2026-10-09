using MemoLingo.Application.Models;
using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Services
{
    public interface IRelativePronounService
    {
        Task<IEnumerable<RelativePronounExerciseModel>> GetSessionAsync(int userId, int take, RelativePronounExerciseType? exerciseType);
        Task<RelativePronounAnswerResultModel> SubmitAnswerAsync(RelativePronounAnswerModel answer);
    }
}
