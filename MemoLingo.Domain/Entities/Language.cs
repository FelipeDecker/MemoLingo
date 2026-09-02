namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um idioma disponível no MemoLingo.
    /// </summary>
    public class Language
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }
    }
}
