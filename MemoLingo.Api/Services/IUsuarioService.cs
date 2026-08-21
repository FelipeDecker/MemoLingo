using MemoLingo.Api.Entities;

namespace MemoLingo.Api.Services
{
    /// <summary>
    /// Contrato de regras de negócio relacionadas ao Usuario.
    /// </summary>
    public interface IUsuarioService
    {
        Task<IEnumerable<Usuario>> ObterTodosAsync();

        Task<Usuario> ObterPorIdAsync(int id);

        Task<Usuario> CriarAsync(Usuario usuario);

        Task<bool> AtualizarAsync(int id, Usuario usuario);

        Task<bool> RemoverAsync(int id);
    }
}
