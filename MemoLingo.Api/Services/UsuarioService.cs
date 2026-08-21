using MemoLingo.Api.Entities;
using MemoLingo.Api.Repositories;

namespace MemoLingo.Api.Services
{
    /// <summary>
    /// Contém toda a lógica de negócio relacionada ao Usuario.
    /// </summary>
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repository;

        public UsuarioService(IGenericRepository<Usuario> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Usuario>> ObterTodosAsync()
        {
            return await _repository.ObterTodosAsync();
        }

        public async Task<Usuario?> ObterPorIdAsync(int id)
        {
            return await _repository.ObterPorIdAsync(id);
        }

        public async Task<Usuario> CriarAsync(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Nome))
            {
                throw new ArgumentException("Nome é obrigatório.", nameof(usuario));
            }

            if (string.IsNullOrWhiteSpace(usuario.Email))
            {
                throw new ArgumentException("Email é obrigatório.", nameof(usuario));
            }

            usuario.CriadoEm = DateTime.UtcNow;
            usuario.Ativo = true;

            await _repository.AdicionarAsync(usuario);
            await _repository.SalvarAlteracoesAsync();

            return usuario;
        }

        public async Task<bool> AtualizarAsync(int id, Usuario usuario)
        {
            var existente = await _repository.ObterPorIdAsync(id);
            if (existente is null)
            {
                return false;
            }

            existente.Nome = usuario.Nome;
            existente.Email = usuario.Email;
            existente.Ativo = usuario.Ativo;

            _repository.Atualizar(existente);
            return await _repository.SalvarAlteracoesAsync();
        }

        public async Task<bool> RemoverAsync(int id)
        {
            var existente = await _repository.ObterPorIdAsync(id);
            if (existente is null)
            {
                return false;
            }

            _repository.Remover(existente);
            return await _repository.SalvarAlteracoesAsync();
        }
    }
}
