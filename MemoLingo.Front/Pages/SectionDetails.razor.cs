using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components;

namespace MemoLingo.Front.Pages
{
    public partial class SectionDetails
    {
        private const string AllFilter = "all";
        private const string MasteredFilter = "mastered";
        private const string LearningFilter = "learning";
        private const string NotPracticedFilter = "not-practiced";

        private static readonly Dictionary<string, string> Filters = new()
        {
            [AllFilter] = "Todas",
            [MasteredFilter] = "Dominadas",
            [LearningFilter] = "Aprendendo",
            [NotPracticedFilter] = "Não praticadas"
        };

        [Parameter]
        public int SectionId { get; set; }

        [Inject]
        private ILessonService LessonService { get; set; }

        private SectionDetailsModel details;
        private bool loading = true;
        private string searchTerm = string.Empty;
        private string selectedFilter = AllFilter;

        private int MasteredPercent =>
            details == null || details.TotalWords == 0
                ? 0
                : (int)Math.Round(details.MasteredWords * 100d / details.TotalWords);

        private List<PracticeWordModel> FilteredWords =>
            (details?.Words ?? new List<PracticeWordModel>())
                .Where(w => MatchesFilter(w, selectedFilter))
                .Where(w => string.IsNullOrWhiteSpace(searchTerm) || MatchesSearch(w, searchTerm.Trim()))
                .ToList();

        protected override async Task OnParametersSetAsync()
        {
            loading = true;
            details = await LessonService.GetSectionDetailsAsync(SectionId);
            loading = false;
        }

        private void OnSearchChanged(ChangeEventArgs args)
        {
            searchTerm = args.Value?.ToString() ?? string.Empty;
        }

        private int CountByFilter(string filter)
        {
            return (details?.Words ?? new List<PracticeWordModel>()).Count(w => MatchesFilter(w, filter));
        }

        private bool MatchesFilter(PracticeWordModel word, string filter)
        {
            var mastered = word.SampleAttemptCount > 0 && word.LearningPercentage >= details.MasteryThreshold;

            return filter switch
            {
                MasteredFilter => mastered,
                LearningFilter => word.SampleAttemptCount > 0 && !mastered,
                NotPracticedFilter => word.SampleAttemptCount == 0,
                _ => true
            };
        }

        private static bool MatchesSearch(PracticeWordModel word, string term)
        {
            return (word.Text?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                || (word.Translation?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private static IEnumerable<string> SplitStructure(string structure)
        {
            return structure
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static string GetAttemptsSummary(PracticeWordModel word)
        {
            var suffix = word.StatsMode == LearningStatsMode.RecentAttempts ? " recentes" : string.Empty;

            return word.SampleAttemptCount == 0
                ? $"sem tentativas{suffix}"
                : $"{word.SampleCorrectCount} de {word.SampleAttemptCount} tentativas{suffix}";
        }

        private string GetProgressCssClass(int percentage)
        {
            if (percentage >= details.MasteryThreshold)
            {
                return "aprendizado-alto";
            }

            return percentage >= 40 ? "aprendizado-medio" : "aprendizado-baixo";
        }

        private static string GetStatusLabel(ProgressStatus status)
        {
            return status switch
            {
                ProgressStatus.Completed => "CONCLUÍDO",
                ProgressStatus.InProgress => "EM ANDAMENTO",
                ProgressStatus.Available => "DISPONÍVEL",
                _ => "BLOQUEADO"
            };
        }

        private static string GetStatusCssClass(ProgressStatus status)
        {
            return status switch
            {
                ProgressStatus.Completed => "concluido",
                ProgressStatus.InProgress or ProgressStatus.Available => "atual",
                _ => "bloqueado"
            };
        }
    }
}
