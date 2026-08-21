namespace MemoLingo.Api.Entities
{
    /// <summary>
    /// Entidade que representa um idioma disponível no MemoLingo.
    /// </summary>
    public class Idioma
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Codigo { get; set; }
    }
}
