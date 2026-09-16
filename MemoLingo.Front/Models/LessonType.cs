namespace MemoLingo.Front.Models
{
    /// <summary>
    /// Define o tipo de uma lição dentro de uma trilha, usado para escolher o ícone da bolinha.
    /// A última lição de cada trilha é tratada como prova final.
    /// </summary>
    public enum LessonType
    {
        Lesson = 1,
        Exam = 2
    }
}
