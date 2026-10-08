using MemoLingo.Application.Models;
using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Services
{
    public interface IPracticeService
    {
        Task<IEnumerable<PracticeWordModel>> GetWordsAsync(int? userId, int? take);
        Task<IEnumerable<PracticeWordModel>> GetWordsByLevelAsync(int? userId, int languageId, CefrLevel cefrLevel);
        Task<IEnumerable<PracticeWordModel>> GetFocusedPracticeWordsAsync(int userId, int take = 10);
        Task<IEnumerable<PracticeWordModel>> GetPhrasalVerbsPracticeAsync(int userId, int take = 10);
        Task<PracticeWordModel> RegisterResultAsync(PracticeResultModel result);
        Task<PracticeWordModel> RegisterWrongAttemptAsync(PracticeWrongAttemptModel model);
    }
}
