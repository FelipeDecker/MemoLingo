using MemoLingo.Services.Models;

namespace MemoLingo.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserModel>> GetAllAsync();
        Task<UserModel> GetByIdAsync(int id);
        Task<UserModel> CreateAsync(UserModel user);
        Task<bool> UpdateAsync(int id, UserModel user);
        Task<bool> RemoveAsync(int id);
    }
}
