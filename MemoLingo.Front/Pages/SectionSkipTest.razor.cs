using System.Diagnostics;
using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MemoLingo.Front.Pages
{
    public partial class SectionSkipTest
    {
        [Inject]
        private ISectionTestActivityService SectionTestActivityService { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        [Parameter]
        public int SectionId { get; set; }

        private const string BlankToken = "{{blank}}";
        private const string EmptyBlank = "_____";

        private readonly Stopwatch stopwatch = new();
        private List<LessonExerciseModel> exercises = new();
        private SectionTestSessionModel session;
        private LessonAnswerResultModel answerResult;
        private SectionTestResultModel result;
        private ElementReference answerInput;
        private ElementReference continueButton;
        private string answer = string.Empty;
        private string errorMessage;
        private string loadError;
        private int currentIndex;
        private int correctCount;
        private int wrongCount;
        private bool loading = true;
        private bool started;
        private bool submitting;
        private bool completing;
        private bool focusAnswer;
        private bool focusContinue;

        private LessonExerciseModel Current => currentIndex < exercises.Count ? exercises[currentIndex] : null;

        private int TotalExercises => session?.TotalExercises ?? 0;

        private int AnsweredCount => correctCount + wrongCount;

        // Sem chance de atingir a nota mínima, o teste é encerrado na próxima questão.
        private bool IsFailed => session is not null && wrongCount > session.MaxWrong;

        private int ProgressPercentage => result is not null
            ? 100
            : TotalExercises == 0 ? 0 : (int)Math.Round((double)AnsweredCount / TotalExercises * 100);

        private bool CanSubmit => !submitting && answerResult is null && !string.IsNullOrWhiteSpace(answer);

        private string FooterCss => answerResult is null
            ? string.Empty
            : answerResult.IsCorrect ? "lesson-footer-certo" : "lesson-footer-errado";

        private string BlankText => answerResult is not null && Current?.ExerciseType == ExerciseType.FillInTheBlank
            ? answerResult.IsCorrect ? answer : answerResult.CorrectAnswer
            : string.IsNullOrWhiteSpace(answer) ? EmptyBlank : answer;

        private string BlankCss => answerResult is null
            ? string.Empty
            : answerResult.IsCorrect ? "lacuna-certa" : "lacuna-corrigida";

        protected override async Task OnParametersSetAsync()
        {
            await StartTestAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (focusAnswer)
            {
                focusAnswer = false;
                await TryFocusAsync(answerInput);
            }
            else if (focusContinue)
            {
                focusContinue = false;
                await TryFocusAsync(continueButton);
            }
        }

        private async Task StartTestAsync()
        {
            loading = true;
            loadError = null;
            result = null;
            session = null;
            started = false;
            exercises = new List<LessonExerciseModel>();

            try
            {
                session = await SectionTestActivityService.StartAsync(SectionId);
                exercises = session.Exercises.OrderBy(e => e.Position).ToList();

                if (exercises.Count == 0)
                {
                    loadError = "Não há questões disponíveis para este teste.";
                }
            }
            catch (InvalidOperationException ex)
            {
                loadError = ex.Message;
            }
            catch (Exception)
            {
                loadError = "Não foi possível carregar o teste. Tente novamente.";
            }

            currentIndex = 0;
            correctCount = 0;
            wrongCount = 0;
            ResetExercise();
            loading = false;
        }

        private void Begin()
        {
            started = true;
            ResetExercise();
        }

        private void SelectOption(string option)
        {
            answer = option;
            errorMessage = null;
        }

        private void OnAnswerInput(ChangeEventArgs args)
        {
            answer = args.Value?.ToString() ?? string.Empty;
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
            stopwatch.Stop();

            try
            {
                answerResult = await SectionTestActivityService.SubmitAnswerAsync(
                    session.StudySessionId,
                    Current,
                    answer,
                    (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue));

                if (answerResult.IsCorrect)
                {
                    correctCount++;
                }
                else
                {
                    wrongCount++;
                }

                focusContinue = true;
            }
            catch (InvalidOperationException ex)
            {
                errorMessage = ex.Message;
                stopwatch.Start();
            }
            catch (Exception)
            {
                errorMessage = "Não foi possível verificar sua resposta. Tente novamente.";
                stopwatch.Start();
            }
            finally
            {
                submitting = false;
            }
        }

        private async Task ContinueAsync()
        {
            if (!IsFailed && currentIndex + 1 < exercises.Count)
            {
                currentIndex++;
                ResetExercise();
                return;
            }

            completing = true;
            errorMessage = null;

            try
            {
                result = await SectionTestActivityService.CompleteAsync(session.StudySessionId);
            }
            catch (InvalidOperationException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception)
            {
                errorMessage = "Não foi possível salvar o resultado do teste. Tente novamente.";
            }
            finally
            {
                completing = false;
            }
        }

        private void ResetExercise()
        {
            answerResult = null;
            answer = string.Empty;
            errorMessage = null;
            focusAnswer = true;
            stopwatch.Restart();
        }

        private void OpenUnlockedSection()
        {
            Navigation.NavigateTo($"/section/{result.SectionId}");
        }

        private void Exit()
        {
            Navigation.NavigateTo("/sections");
        }

        private static async Task TryFocusAsync(ElementReference element)
        {
            try
            {
                await element.FocusAsync();
            }
            catch (Exception)
            {
                // O elemento pode não estar mais na tela (ex.: teste concluído); o foco é apenas conveniência.
            }
        }

        private static string GetInstruction(ExerciseType type) => type switch
        {
            ExerciseType.TranslationToNative => "Traduza para o português",
            ExerciseType.TranslationToTarget => "Escreva em inglês",
            ExerciseType.FillInTheBlank => "Complete a frase com a palavra que falta",
            _ => "Responda"
        };

        private static string GetPlaceholder(ExerciseType type) => type switch
        {
            ExerciseType.TranslationToNative => "Digite em português",
            ExerciseType.FillInTheBlank => "Escolha uma opção ou digite a palavra",
            _ => "Digite em inglês"
        };

        private static (string Before, string After) SplitSentence(string sentence)
        {
            var index = sentence.IndexOf(BlankToken, StringComparison.Ordinal);

            return index < 0
                ? (sentence + " ", string.Empty)
                : (sentence[..index], sentence[(index + BlankToken.Length)..]);
        }
    }
}
