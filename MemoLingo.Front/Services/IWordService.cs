using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    public interface IWordService
    {
        Task<List<Word>> GetWordsForPracticeAsync();
        Task RegisterResultAsync(int wordId, bool correct);
    }
}
