using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MemoLingo.Front.Pages
{
    public partial class NuancePractice
    {
        [Inject]
        private INuanceService NuanceService { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        private const string BlankToken = "{{blank}}";
        private const string EmptyBlank = "_____";

        private List<NuanceExerciseModel> queue = new();
        private NuanceAnswerResultModel result;
        private string answer = string.Empty;
        private string errorMessage;
        private int? selectedWordId;
        private int currentIndex;
        private int sessionCorrectCount;
        private bool showTranslation;
        private bool sessionCompleted;
        private bool submitting;
        private bool loading = true;

        private bool CanSubmit => !submitting && !string.IsNullOrWhiteSpace(answer);

        private int AccuracyPercentage => queue.Count == 0
            ? 0
            : (int)Math.Round((double)sessionCorrectCount / queue.Count * 100, MidpointRounding.AwayFromZero);

        private string BlankText => result is not null
            ? result.CorrectAnswer
            : string.IsNullOrWhiteSpace(answer) ? EmptyBlank : answer;

        private string BlankCss => result is null
            ? string.Empty
            : result.IsCorrect ? "lacuna-certa" : "lacuna-corrigida";

        protected override async Task OnInitializedAsync()
        {
            await StartSessionAsync();
        }

        private async Task StartSessionAsync()
        {
            loading = true;

            queue = await NuanceService.GetSessionAsync();

            currentIndex = 0;
            sessionCorrectCount = 0;
            sessionCompleted = false;
            ResetExercise();
            loading = false;
        }

        private void SelectOption(NuanceOptionModel option)
        {
            answer = option.AnswerForm;
            selectedWordId = option.WordId;
            errorMessage = null;
        }

        private void OnAnswerInput(ChangeEventArgs args)
        {
            answer = args.Value?.ToString() ?? string.Empty;

            // Ao digitar livremente, a escolha pelo botão deixa de valer; o texto passa a ser a resposta.
            selectedWordId = null;
            errorMessage = null;
        }

        private async Task OnAnswerKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && CanSubmit)
            {
                await SubmitAsync();
            }
        }

        private async Task SubmitAsync()
        {
            if (!CanSubmit)
            {
                return;
            }

            submitting = true;
            errorMessage = null;

            try
            {
                result = await NuanceService.SubmitAnswerAsync(queue[currentIndex].Id, answer, selectedWordId);

                if (result.IsCorrect)
                {
                    sessionCorrectCount++;
                }
            }
            catch (Exception)
            {
                errorMessage = "Não foi possível verificar sua resposta. Tente novamente.";
            }
            finally
            {
                submitting = false;
            }
        }

        private void Next()
        {
            if (currentIndex + 1 < queue.Count)
            {
                currentIndex++;
                ResetExercise();
            }
            else
            {
                sessionCompleted = true;
            }
        }

        private void ResetExercise()
        {
            result = null;
            answer = string.Empty;
            selectedWordId = null;
            errorMessage = null;
            showTranslation = false;
        }

        private static (string Before, string After) SplitSentence(string sentence)
        {
            var index = sentence.IndexOf(BlankToken, StringComparison.Ordinal);

            return index < 0
                ? (sentence + " ", string.Empty)
                : (sentence[..index], sentence[(index + BlankToken.Length)..]);
        }

        private static string NuanceItemCss(NuanceItemModel item)
        {
            if (item.IsCorrectAnswer)
            {
                return "nuance-item-certo";
            }

            return item.IsSubmittedAnswer ? "nuance-item-errado" : string.Empty;
        }

        private void GoBackToHub()
        {
            Navigation.NavigateTo("/pratica");
        }
    }
}
