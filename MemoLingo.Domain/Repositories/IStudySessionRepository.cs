using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de leitura do progresso do usuário nas lições.
    /// </summary>
    public interface IStudySessionRepository
    {
        Task<IEnumerable<int>> GetCompletedLessonIdsAsync(int userId);
        Task<IEnumerable<int>> GetInProgressLessonIdsAsync(int userId);
        Task<StudySession> GetOrCreatePracticeSessionAsync(int userId, int languageId);
        Task<StudySession> GetByIdAsync(int id);
        Task<StudySession> StartLessonSessionAsync(int userId, int languageId, int lessonId);
        Task<StudySession> StartSectionTestSessionAsync(int userId, int languageId, int sectionId, int exerciseCount);
        Task AddSkippedLessonSessionsAsync(int userId, int languageId, IEnumerable<int> lessonIds, DateTime completedAt);
        Task<bool> SaveChangesAsync();
    }
}
