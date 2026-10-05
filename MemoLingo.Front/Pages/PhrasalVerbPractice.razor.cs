using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components;

namespace MemoLingo.Front.Pages
{
    public partial class PhrasalVerbPractice
    {
        [Inject]
        private IPracticeService PracticeService { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        private const int XpPerCorrectAnswer = 2;

        private List<PracticeWordModel> queue = new();
        private int currentIndex;
        private int sessionCorrectCount;
        private bool showTranslation;
        private bool sessionCompleted;
        private bool loading = true;

        private int AccuracyPercentage => queue.Count == 0
            ? 0
            : (int)Math.Round((double)sessionCorrectCount / queue.Count * 100, MidpointRounding.AwayFromZero);

        private int EarnedXp => sessionCorrectCount * XpPerCorrectAnswer;

        protected override async Task OnInitializedAsync()
        {
            await StartSessionAsync();
        }

        private async Task StartSessionAsync()
        {
            loading = true;

            // A API prioriza os verbos frasais com mais erros e devolve a lista embaralhada.
            queue = await PracticeService.GetPhrasalVerbsPracticeAsync();

            currentIndex = 0;
            sessionCorrectCount = 0;
            showTranslation = false;
            sessionCompleted = false;
            loading = false;
        }

        private async Task AnswerAsync(bool correct)
        {
            var currentWord = queue[currentIndex];
            await PracticeService.RegisterResultAsync(currentWord.Id, correct);

            if (correct)
            {
                sessionCorrectCount++;
            }

            if (currentIndex + 1 < queue.Count)
            {
                currentIndex++;
                showTranslation = false;
            }
            else
            {
                sessionCompleted = true;
            }
        }

        private void GoBackToHub()
        {
            Navigation.NavigateTo("/pratica");
        }
    }
}
