// Scroll spy da trilha de lições: usa IntersectionObserver para descobrir qual unidade
// está em foco logo abaixo do cabeçalho fixo e avisa o componente Blazor (via JS interop)
// para que o banner do topo passe a exibir a unidade que acabou de entrar em foco.
window.unitScrollSpy = {
    _dotNetRef: null,
    _observer: null,
    _resizeHandler: null,
    _limit: 88,
    _currentId: null,

    start: function (dotNetRef) {
        this.stop();
        this._dotNetRef = dotNetRef;

        this._observe();

        // O tamanho do banner (e, portanto, a linha de corte) muda com a largura da tela.
        this._resizeHandler = () => this._observe();
        window.addEventListener('resize', this._resizeHandler, { passive: true });
    },

    _observe: function () {
        if (!this._dotNetRef) {
            return;
        }

        if (this._observer) {
            this._observer.disconnect();
            this._observer = null;
        }

        const units = document.querySelectorAll('.unit-block');
        if (units.length === 0) {
            return;
        }

        this._limit = this._calculateLimit();

        // A margem negativa no topo recorta a viewport exatamente abaixo do cabeçalho fixo:
        // o callback dispara sempre que um separador de unidade cruza essa linha.
        this._observer = new IntersectionObserver(() => this._update(), {
            rootMargin: `-${this._limit}px 0px 0px 0px`,
            threshold: [0, 1]
        });

        units.forEach((unit) => this._observer.observe(unit));

        this._update();
    },

    // Linha de corte: base do banner fixo quando ele já está grudado no topo.
    _calculateLimit: function () {
        const banner = document.querySelector('.unit-banner');
        if (!banner) {
            return 88;
        }

        const stickyTop = parseFloat(window.getComputedStyle(banner).top) || 0;
        return Math.round(banner.offsetHeight + stickyTop + 8);
    },

    _update: function () {
        if (!this._dotNetRef) {
            return;
        }

        const units = document.querySelectorAll('.unit-block');
        if (units.length === 0) {
            return;
        }

        let activeId = units[0].getAttribute('data-unit-id');

        units.forEach((unit) => {
            if (unit.getBoundingClientRect().top <= this._limit) {
                activeId = unit.getAttribute('data-unit-id');
            }
        });

        if (activeId === null || activeId === this._currentId) {
            return;
        }

        this._currentId = activeId;
        this._dotNetRef.invokeMethodAsync('UpdateActiveUnit', parseInt(activeId, 10));
    },

    stop: function () {
        if (this._observer) {
            this._observer.disconnect();
            this._observer = null;
        }

        if (this._resizeHandler) {
            window.removeEventListener('resize', this._resizeHandler);
            this._resizeHandler = null;
        }

        this._dotNetRef = null;
        this._currentId = null;
    }
};
