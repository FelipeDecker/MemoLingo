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
    }
}
