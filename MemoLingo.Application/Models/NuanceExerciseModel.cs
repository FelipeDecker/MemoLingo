using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class NuanceExerciseModel
    {
        public int Id { get; set; }
        public int SynonymGroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupMeaning { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public string SentenceContext { get; set; }
        public string SentenceTranslation { get; set; }
        public bool IsPriorityReview { get; set; }
        public int GroupProficiencyScore { get; set; }

        public List<NuanceOptionModel> Options { get; set; }
    }
}
