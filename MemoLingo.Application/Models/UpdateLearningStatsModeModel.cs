using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class UpdateLearningStatsModeModel
    {
        public int? UserId { get; set; }
        public LearningStatsMode Mode { get; set; }
    }
}
