using System.Globalization;
using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front.Models;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MemoLingo.Front.Pages
{
    public partial class Learn : IAsyncDisposable
    {
        [Inject]
        private ILessonService LessonService { get; set; }

        [Inject]
        private IJSRuntime JS { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        private List<Unit> units;
        private Unit activeUnit;
        private Section section;
        private DotNetObjectReference<Learn> jsReference;
        private bool scrollSpyPending;

        // Amplitude (px) e frequência da onda senoidal que desenha a trilha sinuosa (zig-zag).
        private const double Amplitude = 70d;
        private const double Frequency = 0.8d;

        /// <summary>
        /// Seção exibida na trilha. Quando ausente, a seção atual do usuário é carregada.
        /// </summary>
        [Parameter]
        public int? SectionId { get; set; }

        private int SectionNumber => section?.Number ?? 1;

        protected override async Task OnParametersSetAsync()
        {
            // Busca a trilha da seção selecionada diretamente da API.
            section = SectionId.HasValue
                ? await LessonService.GetSectionAsync(SectionId.Value)
                : await LessonService.GetCurrentSectionAsync();

            units = section?.Units ?? new List<Unit>();
            activeUnit = units.FirstOrDefault();
            scrollSpyPending = units.Count > 0;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // O primeiro render ocorre ainda durante o carregamento (sem unidades no DOM), por isso
            // o observador é registrado somente depois que a trilha foi de fato renderizada.
            if (scrollSpyPending && units is { Count: > 0 })
            {
                scrollSpyPending = false;
                jsReference ??= DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("unitScrollSpy.start", jsReference);
            }
        }

        /// <summary>
        /// Chamado via JS interop sempre que a unidade em foco na tela muda durante o scroll.
        /// </summary>
        [JSInvokable]
        public void UpdateActiveUnit(int unitId)
        {
            var unit = units?.FirstOrDefault(u => u.Id == unitId);
            if (unit != null && unit != activeUnit)
            {
                activeUnit = unit;
                StateHasChanged();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (jsReference != null)
            {
                try
                {
                    await JS.InvokeVoidAsync("unitScrollSpy.stop");
                }
                catch (JSDisconnectedException)
                {
                    // A conexão já foi encerrada (ex.: navegação/fechamento da página); nada a fazer.
                }

                jsReference.Dispose();
            }
        }

        private int GetUnitNumber(Unit unit) => units == null ? 1 : units.IndexOf(unit) + 1;

        private static bool IsLocked(Lesson lesson) => lesson.Status is ProgressStatus.Locked or ProgressStatus.Abandoned;

        // Cada nó da trilha (Lesson.Id == PathNode.Id) abre uma atividade com as lições do nó.
        private void OpenLesson(Lesson lesson)
        {
            if (!IsLocked(lesson))
            {
                Navigation.NavigateTo($"/lesson/{lesson.Id}");
            }
        }

        /// <summary>
        /// Texto exibido no separador que antecede a unidade (ex.: "Diga de onde você é").
        /// </summary>
        private static string GetSeparatorText(Unit unit) =>
            string.IsNullOrWhiteSpace(unit.Description) ? unit.Name : unit.Description;

        /// <summary>
        /// Formata o deslocamento horizontal em pixels usando cultura invariante,
        /// evitando que a vírgula decimal quebre o CSS inline.
        /// </summary>
        private static string Px(double value) =>
            value.ToString("0.##", CultureInfo.InvariantCulture) + "px";

        /// <summary>
        /// Escurece uma cor hexadecimal para gerar a sombra inferior que dá o efeito 3D nos nós.
        /// </summary>
        private static string Darken(string hexColor, double factor = 0.75d)
        {
            if (string.IsNullOrWhiteSpace(hexColor) || !hexColor.StartsWith("#") || hexColor.Length != 7)
            {
                return "rgba(0, 0, 0, 0.2)";
            }

            var r = (int)(Convert.ToInt32(hexColor.Substring(1, 2), 16) * factor);
            var g = (int)(Convert.ToInt32(hexColor.Substring(3, 2), 16) * factor);
            var b = (int)(Convert.ToInt32(hexColor.Substring(5, 2), 16) * factor);

            return $"#{r:X2}{g:X2}{b:X2}";
        }

        private static string GetStatusClass(ProgressStatus status) => status switch
        {
            ProgressStatus.Completed => "completed",
            ProgressStatus.Available or ProgressStatus.InProgress => "available",
            _ => "locked"
        };

        private static string GetLessonColor(Unit unit, Lesson lesson) => lesson.Status switch
        {
            ProgressStatus.Locked or ProgressStatus.Abandoned => "#e5e5e5",
            _ => unit.PrimaryColor
        };

        private static string GetIcon(Lesson lesson)
        {
            if (lesson.Status is ProgressStatus.Locked or ProgressStatus.Abandoned)
            {
                return "🔒";
            }

            if (lesson.Status == ProgressStatus.Completed)
            {
                return "✓";
            }

            return lesson.Type == LessonType.Exam ? "🏆" : "⭐";
        }
    }
}
