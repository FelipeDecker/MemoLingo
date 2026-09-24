using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    public class UserCourseService : IUserCourseService
    {
        private readonly ICoursesClient _coursesClient;

        public UserCourseService(ICoursesClient coursesClient)
        {
            _coursesClient = coursesClient;
        }

        public async Task<List<UserCourse>> GetUserCoursesAsync()
        {
            var courses = await _coursesClient.GetUserCoursesAsync(null);

            return courses
                .Select(c => new UserCourse
                {
                    LanguageId = c.LanguageId,
                    LanguageCode = c.LanguageCode,
                    LanguageName = c.LanguageName,
                    FlagImage = LanguageFlag.Resolve(c.LanguageCode),
                    Level = c.Level,
                    TotalXp = c.TotalXp,
                    IsActive = c.IsActive
                })
                .ToList();
        }

        public async Task<List<LanguageOption>> GetAvailableLanguagesAsync()
        {
            var languages = await _coursesClient.GetAvailableLanguagesAsync(null);

            return languages
                .Select(l => new LanguageOption
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = l.Name,
                    FlagImage = LanguageFlag.Resolve(l.Code)
                })
                .ToList();
        }

        public Task AddCourseAsync(int languageId)
        {
            return _coursesClient.EnrollAsync(new EnrollCourseModel { LanguageId = languageId });
        }

        public Task SetActiveCourseAsync(int languageId)
        {
            return _coursesClient.SetActiveCourseAsync(new EnrollCourseModel { LanguageId = languageId });
        }
    }
}
