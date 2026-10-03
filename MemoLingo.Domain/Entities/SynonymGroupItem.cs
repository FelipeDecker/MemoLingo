namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Associação entre uma palavra e um grupo de quase-sinônimos, com a explicação
    /// da nuance que diferencia essa palavra das demais do grupo.
    /// </summary>
    public class SynonymGroupItem
    {
        public int Id { get; set; }
        public int SynonymGroupId { get; set; }
        public int WordId { get; set; }
        public string NuanceExplanation { get; set; }
        public int Position { get; set; }

        public SynonymGroup SynonymGroup { get; set; }
        public Word Word { get; set; }
    }
}
