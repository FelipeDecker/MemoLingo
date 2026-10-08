using MemoLingo.Application.Models;

namespace MemoLingo.Application.Services
{
    public interface ISectionTestService
    {
        Task<SectionTestSessionModel> StartAsync(int? userId, int sectionId);
        Task<LessonAnswerResultModel> AnswerAsync(LessonAnswerModel answer);
        Task<SectionTestResultModel> CompleteAsync(int? userId, int studySessionId);
    }
}
