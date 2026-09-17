using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public interface IPracticeService
    {
        Task<List<PracticeWordModel>> GetWordsForPracticeAsync();
        Task<List<PracticeWordModel>> GetDictionaryAsync();
        Task RegisterResultAsync(int wordId, bool correct);
        Task<PracticeWordModel> RegisterWrongAttemptAsync(int wordId);
    }
}
