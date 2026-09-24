namespace MemoLingo.Front.Models
{
    public class UserCourse
    {
        public int LanguageId { get; set; }

        public string LanguageCode { get; set; }

        public string LanguageName { get; set; }

        /// <summary>
        /// Caminho da imagem (SVG) da bandeira do idioma.
        /// </summary>
        public string FlagImage { get; set; }

        public int Level { get; set; }

        public int TotalXp { get; set; }

        public bool IsActive { get; set; }
    }
}
