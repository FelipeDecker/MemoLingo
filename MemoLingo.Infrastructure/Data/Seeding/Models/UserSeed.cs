namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class UserSeed
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string NativeLanguageCode { get; set; }
        public string LearningLanguageCode { get; set; }
        public bool Active { get; set; }
    }
}
