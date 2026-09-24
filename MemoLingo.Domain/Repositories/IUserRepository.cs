using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de leitura de usuários com seus progressos por idioma.
    /// </summary>
    public interface IUserRepository
    {
        Task<User> GetWithProgressesAsync(int id);
        Task<User> GetDefaultAsync();
        Task<IEnumerable<LanguageProgress>> GetProgressesAsync(int userId);
        Task<LanguageProgress> GetProgressAsync(int userId, int languageId);
        Task AddProgressAsync(LanguageProgress progress);
        Task SetActiveCourseAsync(int userId, int languageId);
    }
}
