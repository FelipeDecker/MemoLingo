using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Repositories;

namespace MemoLingo.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _repository;
        private readonly IUserRepository _userRepository;

        public UserService(IGenericRepository<User> repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
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

        public async Task<UserModel> GetCurrentAsync(int? userId)
        {
            var user = await ResolveUserAsync(userId);
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
                Active = true,
                Plan = SubscriptionPlan.Free,
                LearningStatsMode = LearningStatsMode.Total
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

        public async Task<UserModel> UpdateLearningStatsModeAsync(UpdateLearningStatsModeModel model)
        {
            if (!Enum.IsDefined(model.Mode))
            {
                throw new ArgumentException("Modo de estatística inválido.", nameof(model));
            }

            var resolved = await ResolveUserAsync(model.UserId)
                ?? throw new ArgumentException("Usuário não encontrado.", nameof(model));

            var user = await _repository.GetByIdAsync(resolved.Id);

            if (model.Mode == LearningStatsMode.RecentAttempts && user.Plan != SubscriptionPlan.Premium)
            {
                throw new InvalidOperationException("O cálculo pelas últimas tentativas é exclusivo do plano Premium.");
            }

            user.LearningStatsMode = model.Mode;

            _repository.Update(user);
            await _repository.SaveChangesAsync();

            return ToModel(user);
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
                NativeLanguageId = user.NativeLanguageId,
                Plan = user.Plan,
                LearningStatsMode = user.LearningStatsMode
            };
        }

        private async Task<User> ResolveUserAsync(int? userId)
        {
            return userId.HasValue
                ? await _userRepository.GetWithProgressesAsync(userId.Value)
                : await _userRepository.GetDefaultAsync();
        }
    }
}
