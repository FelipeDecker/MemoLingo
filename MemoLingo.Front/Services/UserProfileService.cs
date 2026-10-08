using MemoLingo.Api.Client;
using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUsersClient _usersClient;

        public UserProfileService(IUsersClient usersClient)
        {
            _usersClient = usersClient;
        }

        public Task<UserModel> GetCurrentAsync()
        {
            return ExecuteAsync(() => _usersClient.GetCurrentAsync(null));
        }

        public Task<UserModel> UpdateLearningStatsModeAsync(LearningStatsMode mode)
        {
            return ExecuteAsync(() => _usersClient.UpdateLearningStatsModeAsync(new UpdateLearningStatsModeModel { Mode = mode }));
        }

        // Converte os erros de validação da API em uma mensagem simples para a tela.
        private static async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (MemoLingoException<ErrorResponseModel> ex)
            {
                var message = ex.Result?.Errors;
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? "Não foi possível concluir a operação." : message, ex);
            }
        }
    }
}