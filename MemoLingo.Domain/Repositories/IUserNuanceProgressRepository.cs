using MemoLingo.Domain.Entities;

namespace MemoLingo.Domain.Repositories
{
    /// <summary>
    /// Contrato de acesso à proficiência do usuário em cada grupo de quase-sinônimos.
    /// </summary>
    public interface IUserNuanceProgressRepository
    {
        Task<IEnumerable<UserNuanceProgress>> GetByUserAsync(int userId);
        Task<IEnumerable<UserNuanceProgress>> GetByUserAndGroupsAsync(int userId, IEnumerable<int> groupIds);
        Task AddAsync(UserNuanceProgress progress);
        void Update(UserNuanceProgress progress);
        Task<bool> SaveChangesAsync();
    }
}
