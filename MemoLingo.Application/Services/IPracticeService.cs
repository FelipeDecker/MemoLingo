using MemoLingo.Application.Models;

namespace MemoLingo.Application.Services
{
    public interface IPracticeService
    {
        Task<IEnumerable<PracticeWordModel>> GetWordsAsync(int? userId, int? take);
        Task<PracticeWordModel> RegisterResultAsync(PracticeResultModel result);
    }
}
