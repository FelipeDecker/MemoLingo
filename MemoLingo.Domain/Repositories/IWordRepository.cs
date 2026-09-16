using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de leitura das palavras de um idioma.
    /// </summary>
    public interface IWordRepository
    {
        Task<IEnumerable<Word>> GetByLanguageAsync(int languageId);
        Task<Word> GetByIdAsync(int id);
    }
}
