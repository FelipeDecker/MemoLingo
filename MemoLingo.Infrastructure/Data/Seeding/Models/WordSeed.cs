using MemoLingo.Domain.Enums;

namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class WordSeed
    {
        public string LanguageCode { get; set; }
        public string Text { get; set; }
        public string Translation { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public PartOfSpeech PartOfSpeech { get; set; }
    }
}
