using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class SectionModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Position { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public ProgressStatus Status { get; set; }

        public List<UnitModel> Units { get; set; } = new();
    }
}
