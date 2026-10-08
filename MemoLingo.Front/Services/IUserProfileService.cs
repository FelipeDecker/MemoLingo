using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public interface IUserProfileService
    {
        Task<UserModel> GetCurrentAsync();
        Task<UserModel> UpdateLearningStatsModeAsync(LearningStatsMode mode);
    }
}
