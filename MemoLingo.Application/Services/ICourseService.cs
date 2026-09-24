using MemoLingo.Application.Models;

namespace MemoLingo.Application.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseModel>> GetTrackAsync(int? userId);
        Task<IEnumerable<UserCourseModel>> GetUserCoursesAsync(int? userId);
        Task<IEnumerable<LanguageModel>> GetAvailableLanguagesAsync(int? userId);
        Task<UserCourseModel> EnrollAsync(int? userId, int languageId);
        Task<bool> SetActiveCourseAsync(int? userId, int languageId);
    }
}
