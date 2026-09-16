using MemoLingo.Domain.Enums;

namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class SectionSeed
    {
        public string LanguageCode { get; set; }
        public string CourseName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Position { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public bool Active { get; set; }
    }
}
