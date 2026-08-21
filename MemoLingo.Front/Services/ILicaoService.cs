using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    /// <summary>
    /// Contrato do serviço responsável por obter as unidades e lições da trilha de aprendizado.
    /// </summary>
    public interface ILicaoService
    {
        /// <summary>
        /// Obtém todas as unidades com suas respectivas lições, simulando uma chamada assíncrona a uma API.
        /// </summary>
        Task<List<Unidade>> ObterUnidadesAsync();
    }
}
