using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components;

namespace MemoLingo.Front.Pages
{
    public partial class WordDictionary
    {
        [Inject]
        private IPracticeService PracticeService { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        // Espelha a janela usada pelo back-end para calcular o percentual de aprendizado.
        private const int RecentAttemptsWindow = 100;

        private List<PracticeWordModel> words;
        private string searchTerm = string.Empty;
        private bool updating;

        private List<PracticeWordModel> FilteredWords =>
            words == null
                ? new List<PracticeWordModel>()
                : string.IsNullOrWhiteSpace(searchTerm)
                    ? words
                    : words
                        .Where(w => Matches(w, searchTerm.Trim()))
                        .ToList();

        protected override async Task OnInitializedAsync()
        {
            words = await PracticeService.GetDictionaryAsync();
        }

        private void OnSearchChanged(ChangeEventArgs args)
        {
            searchTerm = args.Value?.ToString() ?? string.Empty;
        }

        private void StartReview()
        {
            Navigation.NavigateTo("/pratica/atividade");
        }

        private void GoBackToHub()
        {
            Navigation.NavigateTo("/pratica");
        }

        /// <summary>
        /// Cada clique registra uma nova tentativa errada da palavra. O back-end grava
        /// o histórico e devolve as estatísticas já recalculadas.
        /// </summary>
        private async Task RegisterWrongAttemptAsync(PracticeWordModel word)
        {
            if (updating)
            {
                return;
            }

            updating = true;

            try
            {
                var updated = await PracticeService.RegisterWrongAttemptAsync(word.Id);

                word.LearningPercentage = updated.LearningPercentage;
                word.CorrectCount = updated.CorrectCount;
                word.WrongCount = updated.WrongCount;
                word.StrengthLevel = updated.StrengthLevel;
                word.RecentAttemptCount = updated.RecentAttemptCount;
                word.RecentCorrectCount = updated.RecentCorrectCount;
                word.RecentWrongCount = updated.RecentWrongCount;
            }
            finally
            {
                updating = false;
            }
        }

        private static bool Matches(PracticeWordModel word, string term)
        {
            return (word.Text?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                || (word.Translation?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private static string GetRecentSummary(PracticeWordModel word)
        {
            return word.RecentAttemptCount == 0
                ? "sem tentativas recentes"
                : $"{word.RecentCorrectCount} de {word.RecentAttemptCount} tentativas recentes";
        }

        private static string GetProgressCssClass(int percentage)
        {
            if (percentage >= 80)
            {
                return "aprendizado-alto";
            }

            return percentage >= 40 ? "aprendizado-medio" : "aprendizado-baixo";
        }
    }
}
