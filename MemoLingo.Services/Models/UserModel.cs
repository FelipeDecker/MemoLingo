namespace MemoLingo.Services.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Active { get; set; }

        public int NativeLanguageId { get; set; }
    }
}
