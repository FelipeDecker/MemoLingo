using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Exercício da coleção de preposições. A frase em inglês traz uma lacuna <c>{{blank}}</c>
    /// para cada preposição cobrada; as respostas de cada lacuna ficam em <see cref="Answers"/>
    /// separadas por '|' (alternativas aceitas na mesma lacuna separadas por '/').
    /// </summary>
    public class PrepositionExercise
    {
        public int Id { get; set; }
        public int LanguageId { get; set; }
        public PrepositionExerciseType ExerciseType { get; set; }
        public PrepositionUsage Usage { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public string Sentence { get; set; }
        public string Translation { get; set; }
        public string Answers { get; set; }
        public string ShownPrepositions { get; set; }
        public string AlternativeSentences { get; set; }
        public string Explanation { get; set; }

        public Language Language { get; set; }
    }
}
