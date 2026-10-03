using Microsoft.AspNetCore.Components;

namespace MemoLingo.Front.Pages
{
    public partial class Home
    {
        [Inject]
        private NavigationManager Navigation { get; set; }

        protected override void OnInitialized()
        {
            // A trilha passou a morar em /learn; a raiz apenas redireciona para ela.
            Navigation.NavigateTo("/learn", replace: true);
        }
    }
}
