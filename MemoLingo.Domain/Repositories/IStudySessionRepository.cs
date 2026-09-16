namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de leitura do progresso do usuário nas lições.
    /// </summary>
    public interface IStudySessionRepository
    {
        Task<IEnumerable<int>> GetCompletedLessonIdsAsync(int userId);
        Task<IEnumerable<int>> GetInProgressLessonIdsAsync(int userId);
    }
}
