using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public interface IRelativePronounService
    {
        Task<List<RelativePronounExerciseModel>> GetSessionAsync(RelativePronounExerciseType? exerciseType);
        Task<RelativePronounAnswerResultModel> SubmitBlanksAsync(int exerciseId, IEnumerable<string> answers);
        Task<RelativePronounAnswerResultModel> SubmitSentenceAsync(int exerciseId, string sentence);
        Task<RelativePronounAnswerResultModel> SubmitMistakeAsync(int exerciseId, int mistakeIndex, string correction);
    }
}
