using MemoLingo.Domain.Enums;

namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class SynonymGroupSeed
    {
        public string LanguageCode { get; set; }
        public string Name { get; set; }
        public string Meaning { get; set; }
        public CefrLevel CefrLevel { get; set; }
    }
}
