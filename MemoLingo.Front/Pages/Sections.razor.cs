using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Models;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components;

namespace MemoLingo.Front.Pages
{
    public partial class Sections
    {
        [Inject]
        private ILessonService LessonService { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        private List<Section> sections;

        protected override async Task OnInitializedAsync()
        {
            sections = await LessonService.GetSectionsAsync();
        }

        private void OpenSection(Section section)
        {
            Navigation.NavigateTo($"/section/{section.Id}");
        }

        // Só é possível pular para a seção bloqueada logo depois da seção que está sendo estudada.
        private bool CanSkipTo(Section section)
        {
            var index = sections.IndexOf(section);

            return index > 0
                && section.Status == ProgressStatus.Locked
                && sections[index - 1].Status is ProgressStatus.Available or ProgressStatus.InProgress;
        }

        private void SkipTo(Section section)
        {
            Navigation.NavigateTo($"/section/{section.Id}/skip-test");
        }
    }
}
