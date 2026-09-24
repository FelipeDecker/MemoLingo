using System.Collections.Generic;

namespace MemoLingo.Front.Models
{
    /// <summary>
    /// Traduz o código do idioma no caminho da imagem da bandeira exibida na interface.
    /// </summary>
    public static class LanguageFlag
    {
        private const string BasePath = "img/flags/";

        private static readonly Dictionary<string, string> Images = new(System.StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = BasePath + "FlagUs.svg",
            ["pt"] = BasePath + "FlagBr.svg",
            ["es"] = BasePath + "FlagEs.svg",
            ["it"] = BasePath + "FlagIt.svg"
        };

        /// <summary>
        /// Retorna o caminho da imagem da bandeira ou <c>null</c> quando o idioma não possui imagem.
        /// </summary>
        public static string Resolve(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return null;
            }

            return Images.TryGetValue(languageCode, out var image) ? image : null;
        }
    }
}
