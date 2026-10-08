using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components;

namespace MemoLingo.Front.Pages
{
    public partial class Perfil
    {
        [Inject]
        private IUserProfileService UserProfileService { get; set; }

        // Espelha a janela usada pelo back-end no modo Premium de últimas tentativas.
        private const int RecentAttemptsWindow = 100;

        private UserModel user;
        private string errorMessage;
        private bool saving;

        private bool IsPremium => user?.Plan == SubscriptionPlan.Premium;

        private bool UsesRecentAttempts => IsPremium && user.LearningStatsMode == LearningStatsMode.RecentAttempts;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                user = await UserProfileService.GetCurrentAsync();
            }
            catch (InvalidOperationException ex)
            {
                errorMessage = ex.Message;
            }
        }

        private async Task OnLearningStatsModeChanged(ChangeEventArgs args)
        {
            if (!IsPremium || saving)
            {
                return;
            }

            var mode = args.Value is true
                ? LearningStatsMode.RecentAttempts
                : LearningStatsMode.Total;

            saving = true;
            errorMessage = null;

            try
            {
                user = await UserProfileService.UpdateLearningStatsModeAsync(mode);
            }
            catch (InvalidOperationException ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                saving = false;
            }
        }
    }
}
