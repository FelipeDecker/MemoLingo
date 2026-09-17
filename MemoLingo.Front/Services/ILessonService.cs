using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    public interface ILessonService
    {
        Task<List<Unit>> GetUnitsAsync();
        Task<List<Section>> GetSectionsAsync();
        Task<Section> GetSectionAsync(int sectionId);
        Task<Section> GetCurrentSectionAsync();
    }
}
