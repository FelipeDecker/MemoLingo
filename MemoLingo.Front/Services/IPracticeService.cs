using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public interface IPracticeService
    {
        Task<List<PracticeWordModel>> GetWordsForPracticeAsync();
        Task RegisterResultAsync(int wordId, bool correct);
    }
}
