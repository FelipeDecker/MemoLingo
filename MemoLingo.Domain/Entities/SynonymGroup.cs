using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Grupo de quase-sinônimos (ex.: guilt / blame / fault) cujas traduções são parecidas,
    /// mas que dependem estritamente do contexto para serem usados corretamente.
    /// </summary>
    public class SynonymGroup
    {
        public int Id { get; set; }
        public int LanguageId { get; set; }
        public string Name { get; set; }
        public string Meaning { get; set; }
        public CefrLevel CefrLevel { get; set; }

        public Language Language { get; set; }
        public ICollection<SynonymGroupItem> Items { get; set; }
        public ICollection<NuanceExercise> Exercises { get; set; }
    }
}
