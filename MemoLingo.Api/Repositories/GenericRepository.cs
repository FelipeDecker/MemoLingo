using MemoLingo.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Api.Repositories
{
    /// <summary>
    /// Implementação genérica de acesso a dados usando EF Core.
    /// </summary>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> ObterPorIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> ObterTodosAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task AdicionarAsync(T entidade)
        {
            await _dbSet.AddAsync(entidade);
        }

        public void Atualizar(T entidade)
        {
            _dbSet.Attach(entidade);
            _context.Entry(entidade).State = EntityState.Modified;
        }

        public void Remover(T entidade)
        {
            _dbSet.Remove(entidade);
        }

        public async Task<bool> SalvarAlteracoesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
