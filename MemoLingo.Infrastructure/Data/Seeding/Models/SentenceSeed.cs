using MemoLingo.Domain.Enums;

namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class SentenceSeed
    {
        public string LanguageCode { get; set; }
        public string Text { get; set; }
        public string Translation { get; set; }
        public CefrLevel CefrLevel { get; set; }
    }
}
