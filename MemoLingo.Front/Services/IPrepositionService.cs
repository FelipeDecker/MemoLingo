using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public interface IPrepositionService
    {
        Task<List<PrepositionExerciseModel>> GetSessionAsync(PrepositionExerciseType? exerciseType);
        Task<PrepositionAnswerResultModel> SubmitBlanksAsync(int exerciseId, IEnumerable<string> answers);
        Task<PrepositionAnswerResultModel> SubmitSentenceAsync(int exerciseId, string sentence);
        Task<PrepositionAnswerResultModel> SubmitMistakeAsync(int exerciseId, int mistakeIndex, string correction);
    }
}
