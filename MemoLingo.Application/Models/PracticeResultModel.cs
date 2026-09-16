namespace MemoLingo.Application.Models
{
    public class PracticeResultModel
    {
        public int UserId { get; set; }

        public int WordId { get; set; }

        public bool Correct { get; set; }
    }
}
