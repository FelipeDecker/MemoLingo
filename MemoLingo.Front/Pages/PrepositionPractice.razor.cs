using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MemoLingo.Front.Pages
{
    public partial class PrepositionPractice
    {
        [Inject]
        private IPrepositionService PrepositionService { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        private const string BlankToken = "{{blank}}";
        private const string MaskedBlank = "_____";

        private static readonly string[] Prepositions = { "in", "on", "at" };

        private static readonly (PrepositionExerciseType? Type, string Label)[] Modes =
        {
            (null, "Misturado"),
            (PrepositionExerciseType.FillInTheBlanks, "Completar"),
            (PrepositionExerciseType.WriteSentence, "Escrever"),
            (PrepositionExerciseType.FindTheMistake, "Encontre o erro")
        };

        private List<PrepositionExerciseModel> queue = new();
        private List<PrepositionAnswerResultModel> missed = new();
        private PrepositionAnswerResultModel result;
        private PrepositionExerciseType? selectedType;
        private string[] blanks = Array.Empty<string>();
        private ElementReference[] blankRefs = Array.Empty<ElementReference>();
        private ElementReference sentenceInput;
        private ElementReference continueButton;
        private string sentenceAnswer = string.Empty;
        private string correction;
        private string errorMessage;
        private int? mistakeIndex;
        private int activeBlank;
        private int currentIndex;
        private int sessionCorrectCount;
        private bool showTranslation;
        private bool showHint;
        private bool sessionCompleted;
        private bool submitting;
        private bool loading = true;
        private bool focusAnswerPending;
        private bool focusContinuePending;

        private PrepositionExerciseModel Current => queue.Count > currentIndex ? queue[currentIndex] : null;

        private bool CanSubmit => !submitting && Current is not null && Current.ExerciseType switch
        {
            PrepositionExerciseType.FillInTheBlanks => blanks.Length > 0 && blanks.All(b => !string.IsNullOrWhiteSpace(b)),
            PrepositionExerciseType.WriteSentence => !string.IsNullOrWhiteSpace(sentenceAnswer),
            PrepositionExerciseType.FindTheMistake => mistakeIndex is not null && !string.IsNullOrWhiteSpace(correction),
            _ => false
        };

        private int AccuracyPercentage => queue.Count == 0
            ? 0
            : (int)Math.Round((double)sessionCorrectCount / queue.Count * 100, MidpointRounding.AwayFromZero);

        private string ResultTitle => result is { IsCorrect: true } ? "Isso aí! 🎉" : "Não foi dessa vez...";

        protected override async Task OnInitializedAsync()
        {
            await StartSessionAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (focusContinuePending)
            {
                focusContinuePending = false;
                await TryFocusAsync(continueButton);
            }
            else if (focusAnswerPending)
            {
                focusAnswerPending = false;

                if (Current?.ExerciseType == PrepositionExerciseType.FillInTheBlanks && blankRefs.Length > 0)
                {
                    await TryFocusAsync(blankRefs[0]);
                }
                else if (Current?.ExerciseType == PrepositionExerciseType.WriteSentence)
                {
                    await TryFocusAsync(sentenceInput);
                }
            }
        }

        private async Task StartSessionAsync()
        {
            loading = true;

            try
            {
                queue = await PrepositionService.GetSessionAsync(selectedType);
            }
            catch (Exception)
            {
                queue = new List<PrepositionExerciseModel>();
            }

            missed = new List<PrepositionAnswerResultModel>();
            currentIndex = 0;
            sessionCorrectCount = 0;
            sessionCompleted = false;
            ResetExercise();
            loading = false;
        }

        private async Task ChangeModeAsync(PrepositionExerciseType? type)
        {
            if (selectedType == type && !sessionCompleted)
            {
                return;
            }

            selectedType = type;
            await StartSessionAsync();
        }

        private void OnBlankInput(int index, ChangeEventArgs args)
        {
            blanks[index] = args.Value?.ToString() ?? string.Empty;
            errorMessage = null;
        }

        private void OnSentenceInput(ChangeEventArgs args)
        {
            sentenceAnswer = args.Value?.ToString() ?? string.Empty;
            errorMessage = null;
        }

        // Preenche a lacuna ativa com a preposição escolhida e avança para a próxima lacuna vazia.
        private async Task ChooseForActiveBlankAsync(string preposition)
        {
            if (blanks.Length == 0)
            {
                return;
            }

            var index = Math.Clamp(activeBlank, 0, blanks.Length - 1);
            blanks[index] = preposition;
            errorMessage = null;

            var next = FindNextEmptyBlank(index);
            if (next >= 0)
            {
                activeBlank = next;
                await TryFocusAsync(blankRefs[next]);
            }
        }

        private void SelectMistake(int index)
        {
            mistakeIndex = index;
            correction = null;
            errorMessage = null;
        }

        private async Task OnAnswerKeyDown(KeyboardEventArgs args)
        {
            if (args.Key != "Enter")
            {
                return;
            }

            if (CanSubmit)
            {
                await SubmitAsync();
                return;
            }

            if (Current?.ExerciseType == PrepositionExerciseType.FillInTheBlanks)
            {
                var next = FindNextEmptyBlank(activeBlank);
                if (next >= 0)
                {
                    activeBlank = next;
                    await TryFocusAsync(blankRefs[next]);
                }
            }
        }

        private async Task SubmitAsync()
        {
            if (!CanSubmit)
            {
                return;
            }

            var current = Current;
            submitting = true;
            errorMessage = null;

            try
            {
                result = current.ExerciseType switch
                {
                    PrepositionExerciseType.FillInTheBlanks => await PrepositionService.SubmitBlanksAsync(current.Id, blanks.Select(b => b.Trim())),
                    PrepositionExerciseType.WriteSentence => await PrepositionService.SubmitSentenceAsync(current.Id, sentenceAnswer.Trim()),
                    _ => await PrepositionService.SubmitMistakeAsync(current.Id, mistakeIndex.Value, correction)
                };

                if (result.IsCorrect)
                {
                    sessionCorrectCount++;
                }
                else
                {
                    missed.Add(result);
                }

                focusContinuePending = true;
            }
            catch (InvalidOperationException ex)
            {
                errorMessage = ex.Message;
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
            var blankCount = Current?.BlankCount ?? 0;

            result = null;
            blanks = Enumerable.Repeat(string.Empty, blankCount).ToArray();
            blankRefs = new ElementReference[blankCount];
            activeBlank = 0;
            sentenceAnswer = string.Empty;
            mistakeIndex = null;
            correction = null;
            errorMessage = null;
            showTranslation = false;
            showHint = false;
            focusAnswerPending = Current is not null;
        }

        private int FindNextEmptyBlank(int fromIndex)
        {
            for (var offset = 1; offset <= blanks.Length; offset++)
            {
                var candidate = (fromIndex + offset) % blanks.Length;
                if (string.IsNullOrWhiteSpace(blanks[candidate]))
                {
                    return candidate;
                }
            }

            return -1;
        }

        private PrepositionBlankResultModel BlankResult(int index)
        {
            return result?.Blanks?.FirstOrDefault(b => b.Index == index);
        }

        private static List<string> SplitSentence(string sentence)
        {
            return (sentence ?? string.Empty).Split(BlankToken).ToList();
        }

        private static string MaskedSentence(List<string> parts)
        {
            return string.Join(MaskedBlank, parts);
        }

        // Uma preposição no início da frase é exibida com inicial maiúscula.
        private static string DisplayWord(List<string> parts, int index, string word)
        {
            if (index == 0 && parts.Count > 0 && string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrEmpty(word))
            {
                return char.ToUpperInvariant(word[0]) + word[1..];
            }

            return word;
        }

        private static List<(string Text, bool IsPreposition)> BuildSegments(string sentence, List<string> answers)
        {
            var parts = SplitSentence(sentence);
            var segments = new List<(string Text, bool IsPreposition)>();

            for (var i = 0; i < parts.Count; i++)
            {
                if (i > 0)
                {
                    var answer = i - 1 < answers.Count ? answers[i - 1] : string.Empty;
                    segments.Add((DisplayWord(parts, i - 1, answer), true));
                }

                segments.Add((parts[i], false));
            }

            return segments;
        }

        private static string TypeLabel(PrepositionExerciseType type)
        {
            return type switch
            {
                PrepositionExerciseType.FillInTheBlanks => "Complete",
                PrepositionExerciseType.WriteSentence => "Escreva a frase",
                PrepositionExerciseType.FindTheMistake => "Encontre o erro",
                _ => string.Empty
            };
        }

        private static string UsageLabel(PrepositionUsage usage)
        {
            return usage switch
            {
                PrepositionUsage.Time => "Tempo",
                PrepositionUsage.Place => "Lugar",
                PrepositionUsage.Expression => "Expressão",
                PrepositionUsage.Mixed => "Misto",
                _ => string.Empty
            };
        }

        private static async Task TryFocusAsync(ElementReference element)
        {
            try
            {
                await element.FocusAsync();
            }
            catch (Exception)
            {
                // O elemento pode não estar mais na tela (ex.: troca de exercício); o foco é apenas conveniência.
            }
        }

        private void GoBackToHub()
        {
            Navigation.NavigateTo("/pratica");
        }
    }
}
