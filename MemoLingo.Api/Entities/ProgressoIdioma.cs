namespace MemoLingo.Api.Entities
{
    /// <summary>
    /// Entidade de associação entre Usuario e Idioma, representando o progresso
    /// de um usuário em um idioma que está aprendendo.
    /// </summary>
    public class ProgressoIdioma
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; }

        public int IdiomaId { get; set; }

        public Idioma Idioma { get; set; }

        public int Nivel { get; set; }

        public int XpTotal { get; set; }

        public bool IsCursoAtivo { get; set; }

        public int TotalPalavrasAprendidas { get; set; }

        public int TotalLicoesConcluidas { get; set; }

        public int OfensivaAtualDias { get; set; }

        public DateTime CriadoEm { get; set; }
    }
}
