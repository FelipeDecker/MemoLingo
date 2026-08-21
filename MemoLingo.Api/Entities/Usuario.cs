namespace MemoLingo.Api.Entities
{
    /// <summary>
    /// Entidade que representa um usuário do MemoLingo.
    /// </summary>
    public class Usuario
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Email { get; set; }

        public string SenhaHash { get; set; }

        public DateTime CriadoEm { get; set; }

        public bool Ativo { get; set; }

        public int IdiomaMaternoId { get; set; }

        public Idioma IdiomaMaterno { get; set; }

        public ICollection<ProgressoIdioma> ProgressosIdioma { get; set; }
    }
}
