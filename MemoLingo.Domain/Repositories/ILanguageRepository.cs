using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de leitura dos idiomas disponíveis no MemoLingo.
    /// </summary>
    public interface ILanguageRepository
    {
        Task<IEnumerable<Language>> GetAllAsync();
        Task<Language> GetByIdAsync(int id);
    }
}
