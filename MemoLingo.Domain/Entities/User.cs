namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um usuário do MemoLingo.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Active { get; set; }

        public int NativeLanguageId { get; set; }

        public Language NativeLanguage { get; set; }

        public ICollection<LanguageProgress> LanguageProgresses { get; set; }
    }
}
