namespace MemoLingo.Api.Entities
{
    /// <summary>
    /// Entidade de associação entre Usuario e Idioma, representando os idiomas
    /// que um usuário está aprendendo.
    /// </summary>
    public class UsuarioIdioma
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; }

        public int IdiomaId { get; set; }

        public Idioma Idioma { get; set; }

        public DateTime CriadoEm { get; set; }
    }
}
