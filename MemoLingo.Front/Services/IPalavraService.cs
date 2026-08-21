using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    /// <summary>
    /// Contrato do serviço responsável por fornecer as palavras do usuário para o
    /// modo de prática, priorizando aquelas com mais erros (diferencial do app).
    /// </summary>
    public interface IPalavraService
    {
        /// <summary>
        /// Obtém as palavras do usuário ordenadas da maior para a menor quantidade
        /// de erros, para montar a sessão de prática.
        /// </summary>
        Task<List<Palavra>> ObterPalavrasParaPraticaAsync();

        /// <summary>
        /// Registra o resultado de uma tentativa de prática, atualizando o
        /// contador de acertos ou erros da palavra informada.
        /// </summary>
        Task RegistrarResultadoAsync(int palavraId, bool acertou);
    }
}
