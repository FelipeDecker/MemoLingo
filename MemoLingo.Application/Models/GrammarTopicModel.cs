namespace MemoLingo.Application.Models
{
    public class GrammarTopicModel
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public string Explanation { get; set; }
        public string Structure { get; set; }
        public bool IsMandatory { get; set; }

        public List<string> Examples { get; set; } = new();
    }
}
