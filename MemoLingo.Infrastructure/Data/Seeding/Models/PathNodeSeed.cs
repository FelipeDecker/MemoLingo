using MemoLingo.Domain.Enums;

namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class PathNodeSeed
    {
        public string LanguageCode { get; set; }
        public string CourseName { get; set; }
        public string UnitTitle { get; set; }
        public NodeType NodeType { get; set; }
        public int Position { get; set; }
        public int TotalLessons { get; set; }
        public bool Active { get; set; }
    }
}
