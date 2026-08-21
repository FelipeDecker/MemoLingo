namespace MemoLingo.Front.Models
{
    /// <summary>
    /// Representa uma lição (bolinha) dentro de uma trilha de aprendizado.
    /// </summary>
    public class Licao
    {
        public int Id { get; set; }

        /// <summary>
        /// Id da unidade à qual esta lição pertence, usado para agrupar as lições por unidade.
        /// </summary>
        public int UnidadeId { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public TipoLicao Tipo { get; set; }

        public StatusLicao Status { get; set; }

        /// <summary>
        /// Ordem de exibição da lição dentro da unidade (define a posição na trilha em zigue-zague).
        /// </summary>
        public int Ordem { get; set; }
    }
}
