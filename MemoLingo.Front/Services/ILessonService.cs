using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    public interface ILessonService
    {
        Task<List<Unit>> GetUnitsAsync();
    }
}
