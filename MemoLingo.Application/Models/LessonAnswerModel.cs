using System.ComponentModel.DataAnnotations;
using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class LessonAnswerModel
    {
        public int? UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int StudySessionId { get; set; }

        [Range(1, int.MaxValue)]
        public int SentenceId { get; set; }

        public ExerciseType ExerciseType { get; set; }
        public int? BlankWordId { get; set; }
        public string Answer { get; set; }

        [Range(0, int.MaxValue)]
        public int ResponseTimeMs { get; set; }
    }
}