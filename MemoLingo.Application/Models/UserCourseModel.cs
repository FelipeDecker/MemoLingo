namespace MemoLingo.Application.Models
{
    public class UserCourseModel
    {
        public int LanguageId { get; set; }

        public string LanguageCode { get; set; }

        public string LanguageName { get; set; }

        public int Level { get; set; }

        public int TotalXp { get; set; }

        public bool IsActive { get; set; }
    }
}
