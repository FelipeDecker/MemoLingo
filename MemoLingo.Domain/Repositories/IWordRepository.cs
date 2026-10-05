using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de leitura das palavras de um idioma.
    /// </summary>
    public interface IWordRepository
    {
        Task<IEnumerable<Word>> GetByLanguageAsync(int languageId);
        Task<IEnumerable<Word>> GetLearnedByUserAsync(int userId, int languageId);
        Task<IEnumerable<Word>> GetByLanguageAndPartOfSpeechAsync(int languageId, PartOfSpeech partOfSpeech);
        Task<Word> GetByIdAsync(int id);
    }
}
