using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class UnitModel
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Topic { get; set; }
        public int Position { get; set; }
        public ProgressStatus Status { get; set; }

        public List<PathNodeModel> PathNodes { get; set; } = new();
    }
}
