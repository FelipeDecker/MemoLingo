using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public interface ISectionTestActivityService
    {
        Task<SectionTestSessionModel> StartAsync(int sectionId);
        Task<LessonAnswerResultModel> SubmitAnswerAsync(int studySessionId, LessonExerciseModel exercise, string answer, int responseTimeMs);
        Task<SectionTestResultModel> CompleteAsync(int studySessionId);
    }
}
