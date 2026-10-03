namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Exercício de discriminação contextual: uma frase detalhada com a lacuna
    /// <c>{{blank}}</c> que deve ser preenchida com a palavra correta do grupo.
    /// </summary>
    public class NuanceExercise
    {
        public int Id { get; set; }
        public int SynonymGroupId { get; set; }
        public int TargetWordId { get; set; }
        public string SentenceContext { get; set; }
        public string SentenceTranslation { get; set; }
        public string AcceptedAnswers { get; set; }
        public string Explanation { get; set; }

        public SynonymGroup SynonymGroup { get; set; }
        public Word TargetWord { get; set; }
    }
}
