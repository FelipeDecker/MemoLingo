using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de acesso ao progresso do usuário em cada nó da trilha.
    /// </summary>
    public interface IUserNodeProgressRepository
    {
        Task<UserNodeProgress> GetAsync(int userId, int pathNodeId);
        Task AddAsync(UserNodeProgress progress);
        Task<bool> SaveChangesAsync();
    }
}
