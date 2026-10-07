using System.Diagnostics;
using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MemoLingo.Front.Pages
{
    public partial class LessonActivity
    {
        [Inject]
        private ILessonActivityService LessonActivityService { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        [Parameter]
        public int PathNodeId { get; set; }

        private const string BlankToken = "{{blank}}";
        private const string EmptyBlank = "_____";

        private readonly Stopwatch stopwatch = new();
        private List<QueueItem> queue = new();
        private LessonSessionModel session;
        private LessonAnswerResultModel result;
        private LessonCompletionModel completion;
        private ElementReference answerInput;
        private ElementReference continueButton;
        private string answer = string.Empty;
        private string errorMessage;
        private string loadError;
        private int currentIndex;
        private int correctCount;
        private bool loading = true;
        private bool submitting;
        private bool completing;
        private bool focusAnswer;
        private bool focusContinue;

        private QueueItem Current => currentIndex < queue.Count ? queue[currentIndex] : null;

        private int TotalExercises => session?.Exercises.Count ?? 0;

        private int ProgressPercentage => completion is not null
            ? 100
            : TotalExercises == 0 ? 0 : (int)Math.Round((double)correctCount / TotalExercises * 100);

        private bool CanSubmit => !submitting && result is null && !string.IsNullOrWhiteSpace(answer);

        private string FooterCss => result is null
            ? string.Empty
            : result.IsCorrect ? "lesson-footer-certo" : "lesson-footer-errado";

        private string BlankText => result is not null && Current?.Exercise.ExerciseType == ExerciseType.FillInTheBlank
            ? result.IsCorrect ? answer : result.CorrectAnswer
            : string.IsNullOrWhiteSpace(answer) ? EmptyBlank : answer;

        private string BlankCss => result is null
            ? string.Empty
            : result.IsCorrect ? "lacuna-certa" : "lacuna-corrigida";

        protected override async Task OnParametersSetAsync()
        {
            await StartLessonAsync();
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

        private async Task StartLessonAsync()
        {
            loading = true;
            loadError = null;
            completion = null;
            session = null;
            queue = new List<QueueItem>();

            try
            {
                session = await LessonActivityService.StartAsync(PathNodeId);
                queue = session.Exercises
                    .OrderBy(e => e.Position)
                    .Select(e => new QueueItem(e, false))
                    .ToList();

                if (queue.Count == 0)
                {
                    loadError = "Esta lição ainda não possui exercícios.";
                }
            }
            catch (InvalidOperationException ex)
            {
                loadError = ex.Message;
            }
            catch (Exception)
            {
                loadError = "Não foi possível carregar a lição. Tente novamente.";
            }

            currentIndex = 0;
            correctCount = 0;
            ResetExercise();
            loading = false;
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
                result = await LessonActivityService.SubmitAnswerAsync(
                    session.StudySessionId,
                    Current.Exercise,
                    answer,
                    (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue));

                if (result.IsCorrect)
                {
                    correctCount++;
                }
                else
                {
                    // Como no Duolingo, o exercício errado volta para o fim da fila até ser acertado.
                    queue.Add(new QueueItem(Current.Exercise, true));
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
            if (currentIndex + 1 < queue.Count)
            {
                currentIndex++;
                ResetExercise();
                return;
            }

            completing = true;
            errorMessage = null;

            try
            {
                completion = await LessonActivityService.CompleteAsync(session.StudySessionId);
            }
            catch (InvalidOperationException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception)
            {
                errorMessage = "Não foi possível salvar o progresso da lição. Tente novamente.";
            }
            finally
            {
                completing = false;
            }
        }

        private void ResetExercise()
        {
            result = null;
            answer = string.Empty;
            errorMessage = null;
            focusAnswer = true;
            stopwatch.Restart();
        }

        private void Exit()
        {
            Navigation.NavigateTo(session is null ? "/learn" : $"/section/{session.SectionId}");
        }

        private static async Task TryFocusAsync(ElementReference element)
        {
            try
            {
                await element.FocusAsync();
            }
            catch (Exception)
            {
                // O elemento pode não estar mais na tela (ex.: lição concluída); o foco é apenas conveniência.
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

        private sealed record QueueItem(LessonExerciseModel Exercise, bool IsRetry);
    }
}
