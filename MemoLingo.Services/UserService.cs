using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;
using MemoLingo.Services.Models;

namespace MemoLingo.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _repository;

        public UserService(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UserModel>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();
            return users.Select(ToModel).ToList();
        }

        public async Task<UserModel> GetByIdAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            return user is null ? null : ToModel(user);
        }

        public async Task<UserModel> CreateAsync(UserModel user)
        {
            if (string.IsNullOrWhiteSpace(user.Name))
            {
                throw new ArgumentException("Name é obrigatório.", nameof(user));
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentException("Email é obrigatório.", nameof(user));
            }

            var entity = new User
            {
                Name = user.Name,
                Email = user.Email,
                NativeLanguageId = user.NativeLanguageId,
                CreatedAt = DateTime.UtcNow,
                Active = true
            };

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            return ToModel(entity);
        }

        public async Task<bool> UpdateAsync(int id, UserModel user)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing is null)
            {
                return false;
            }

            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.Active = user.Active;

            _repository.Update(existing);
            return await _repository.SaveChangesAsync();
        }

        public async Task<bool> RemoveAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing is null)
            {
                return false;
            }

            _repository.Remove(existing);
            return await _repository.SaveChangesAsync();
        }

        private static UserModel ToModel(User user)
        {
            return new UserModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Active = user.Active,
                NativeLanguageId = user.NativeLanguageId
            };
        }
    }
}
