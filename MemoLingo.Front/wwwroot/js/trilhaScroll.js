// Observa o scroll da página e informa ao componente Blazor (via JS interop)
// qual "unidade" da trilha está atualmente em foco, para que o cabeçalho fixo
// no topo seja substituído pelo título da próxima unidade (efeito Duolingo).
window.trilhaScroll = {
    _dotNetRef: null,
    _handler: null,
    _limitePx: 72, // altura aproximada do cabeçalho fixo, usada como linha de corte

    iniciar: function (dotNetRef) {
        this.finalizar();
        this._dotNetRef = dotNetRef;

        this._handler = () => this._atualizar();
        window.addEventListener('scroll', this._handler, { passive: true });
        window.addEventListener('resize', this._handler, { passive: true });

        // Calcula o estado inicial assim que a página carrega.
        this._atualizar();
    },

    _atualizar: function () {
        if (!this._dotNetRef) {
            return;
        }

        const marcadores = document.querySelectorAll('.unidade-marcador');
        let unidadeAtualId = null;

        marcadores.forEach((marcador) => {
            const posicao = marcador.getBoundingClientRect().top;
            if (posicao <= this._limitePx) {
                unidadeAtualId = marcador.getAttribute('data-unidade-id');
            }
        });

        if (unidadeAtualId === null && marcadores.length > 0) {
            // Ainda não passou pelo primeiro marcador: usa a primeira unidade.
            unidadeAtualId = marcadores[0].getAttribute('data-unidade-id');
        }

        if (unidadeAtualId !== null) {
            this._dotNetRef.invokeMethodAsync('UpdateActiveUnit', parseInt(unidadeAtualId, 10));
        }
    },

    finalizar: function () {
        if (this._handler) {
            window.removeEventListener('scroll', this._handler);
            window.removeEventListener('resize', this._handler);
            this._handler = null;
        }
        this._dotNetRef = null;
    }
};
