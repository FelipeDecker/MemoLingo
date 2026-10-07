using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public interface ILessonActivityService
    {
        Task<LessonSessionModel> StartAsync(int pathNodeId);
        Task<LessonAnswerResultModel> SubmitAnswerAsync(int studySessionId, LessonExerciseModel exercise, string answer, int responseTimeMs);
        Task<LessonCompletionModel> CompleteAsync(int studySessionId);
    }
}
