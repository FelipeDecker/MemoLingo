using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de leitura das frases usadas nos exercícios das lições.
    /// </summary>
    public interface ISentenceRepository
    {
        Task<IEnumerable<Sentence>> GetByLevelAsync(int languageId, CefrLevel cefrLevel);
        Task<Sentence> GetByIdAsync(int id);
    }
}
