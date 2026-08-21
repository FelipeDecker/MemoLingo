namespace MemoLingo.Api.Repositories
{
    /// <summary>
    /// Contrato genérico de acesso a dados para entidades do domínio.
    /// </summary>
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> ObterPorIdAsync(int id);

        Task<IEnumerable<T>> ObterTodosAsync();

        Task AdicionarAsync(T entidade);

        void Atualizar(T entidade);

        void Remover(T entidade);

        Task<bool> SalvarAlteracoesAsync();
    }
}
