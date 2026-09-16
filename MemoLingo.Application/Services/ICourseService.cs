using MemoLingo.Application.Models;

namespace MemoLingo.Application.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseModel>> GetTrackAsync(int? userId);
    }
}
