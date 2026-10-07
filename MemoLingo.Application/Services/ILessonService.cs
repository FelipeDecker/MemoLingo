using MemoLingo.Application.Models;

namespace MemoLingo.Application.Services
{
    public interface ILessonService
    {
        Task<LessonSessionModel> StartAsync(int? userId, int pathNodeId);
        Task<LessonAnswerResultModel> AnswerAsync(LessonAnswerModel answer);
        Task<LessonCompletionModel> CompleteAsync(int? userId, int studySessionId);
    }
}
