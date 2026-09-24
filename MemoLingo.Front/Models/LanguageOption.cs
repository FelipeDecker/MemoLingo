namespace MemoLingo.Front.Models
{
    public class LanguageOption
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Caminho da imagem (SVG) da bandeira do idioma.
        /// </summary>
        public string FlagImage { get; set; }
    }
}
