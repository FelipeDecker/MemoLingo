using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class LessonExerciseModel
    {
        public int Position { get; set; }
        public int SentenceId { get; set; }
        public ExerciseType ExerciseType { get; set; }
        public string Prompt { get; set; }
        public string Hint { get; set; }
        public int? BlankWordId { get; set; }

        public List<string> Options { get; set; } = new();
    }
}