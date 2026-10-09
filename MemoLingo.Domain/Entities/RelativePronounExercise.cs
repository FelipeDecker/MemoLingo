using MemoLingo.Domain.Enums;

namespace MemoLingo.Domain.Entities
{
    /// <summary>
    /// Exercício da coleção de pronomes relativos. A frase em inglês traz uma lacuna <c>{{blank}}</c>
    /// para cada pronome cobrado; as respostas de cada lacuna ficam em <see cref="Answers"/>
    /// separadas por '|' (alternativas aceitas na mesma lacuna separadas por '/').
    /// </summary>
    public class RelativePronounExercise
    {
        public int Id { get; set; }
        public int LanguageId { get; set; }
        public RelativePronounExerciseType ExerciseType { get; set; }
        public RelativePronounUsage Usage { get; set; }
        public CefrLevel CefrLevel { get; set; }
        public string Sentence { get; set; }
        public string Translation { get; set; }
        public string Answers { get; set; }
        public string ShownPronouns { get; set; }
        public string AlternativeSentences { get; set; }
        public string Explanation { get; set; }

        public Language Language { get; set; }
    }
}
