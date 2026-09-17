using MemoLingo.Application.Models;

namespace MemoLingo.Application.Services
{
    public interface IPracticeService
    {
        Task<IEnumerable<PracticeWordModel>> GetWordsAsync(int? userId, int? take);
        Task<IEnumerable<PracticeWordModel>> GetFocusedPracticeWordsAsync(int userId, int take = 10);
        Task<PracticeWordModel> RegisterResultAsync(PracticeResultModel result);
        Task<PracticeWordModel> RegisterWrongAttemptAsync(PracticeWrongAttemptModel model);
    }
}
