namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato genérico de acesso a dados para entidades do domínio.
    /// </summary>
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<bool> SaveChangesAsync();
    }
}
