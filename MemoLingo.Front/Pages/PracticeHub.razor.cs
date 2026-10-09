using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MemoLingo.Front.Pages
{
    public partial class PracticeHub
    {
        [Inject]
        private NavigationManager Navigation { get; set; }

        private void StartFocusedPractice()
        {
            // Sem tela intermediária: o usuário cai direto na atividade interativa.
            Navigation.NavigateTo("/pratica/atividade");
        }

        private void OpenDictionary()
        {
            Navigation.NavigateTo("/dicionario");
        }

        private void HandleDictionaryKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" || args.Key == " ")
            {
                OpenDictionary();
            }
        }

        private void OpenNuances()
        {
            Navigation.NavigateTo("/pratica/nuances");
        }

        private void HandleNuancesKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" || args.Key == " ")
            {
                OpenNuances();
            }
        }

        private void OpenPhrasalVerbs()
        {
            Navigation.NavigateTo("/pratica/verbos-frasais");
        }

        private void HandlePhrasalVerbsKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" || args.Key == " ")
            {
                OpenPhrasalVerbs();
            }
        }

        private void OpenPrepositions()
        {
            Navigation.NavigateTo("/pratica/preposicoes");
        }

        private void HandlePrepositionsKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" || args.Key == " ")
            {
                OpenPrepositions();
            }
        }

        private void OpenRelativePronouns()
        {
            Navigation.NavigateTo("/pratica/pronomes-relativos");
        }

        private void HandleRelativePronounsKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" || args.Key == " ")
            {
                OpenRelativePronouns();
            }
        }
    }
}
