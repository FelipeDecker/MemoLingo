using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    public interface IUserCourseService
    {
        Task<List<UserCourse>> GetUserCoursesAsync();
        Task<List<LanguageOption>> GetAvailableLanguagesAsync();
        Task AddCourseAsync(int languageId);
        Task SetActiveCourseAsync(int languageId);
    }
}
