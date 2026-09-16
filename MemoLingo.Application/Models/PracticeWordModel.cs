using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Models
{
    public class PracticeWordModel
    {
        public int Id { get; set; }

        public int LanguageId { get; set; }

        public string Text { get; set; }

        public string Translation { get; set; }

        public CefrLevel CefrLevel { get; set; }

        public PartOfSpeech PartOfSpeech { get; set; }

        public int CorrectCount { get; set; }

        public int WrongCount { get; set; }

        public int StrengthLevel { get; set; }

        public DateTime? LastReview { get; set; }

        public DateTime? NextReview { get; set; }
    }
}
