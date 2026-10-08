namespace MemoLingo.Infrastructure.Data.Seeding.Models
{
    public class GrammarTopicSeed
    {
        public string Title { get; set; }
        public string Explanation { get; set; }
        public string Structure { get; set; }
        public List<string> Examples { get; set; }
        public bool IsMandatory { get; set; }
    }
}
