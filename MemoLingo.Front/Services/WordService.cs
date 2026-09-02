using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    public class WordService : IWordService
    {
        private readonly List<Word> words = GetMockWords();

        public async Task<List<Word>> GetWordsForPracticeAsync()
        {
            // Simula a latência de uma chamada real a uma API.
            await Task.Delay(300);

            // Diferencial do app: prioriza as palavras com mais erros (menor taxa de acerto primeiro).
            return words
                .OrderByDescending(w => w.WrongCount)
                .ThenBy(w => w.AccuracyRate)
                .ToList();
        }

        public Task RegisterResultAsync(int wordId, bool correct)
        {
            var word = words.FirstOrDefault(w => w.Id == wordId);
            if (word != null)
            {
                if (correct)
                {
                    word.CorrectCount++;
                }
                else
                {
                    word.WrongCount++;
                }
            }

            return Task.CompletedTask;
        }

        private static List<Word> GetMockWords()
        {
            return new List<Word>
            {
                new Word { Id = 1, Text = "obrigado", Translation = "thank you", WrongCount = 7, CorrectCount = 2 },
                new Word { Id = 2, Text = "convite", Translation = "invitation", WrongCount = 5, CorrectCount = 1 },
                new Word { Id = 3, Text = "ajuda", Translation = "help", WrongCount = 4, CorrectCount = 3 },
                new Word { Id = 4, Text = "comida", Translation = "food", WrongCount = 3, CorrectCount = 5 },
                new Word { Id = 5, Text = "bebida", Translation = "drink", WrongCount = 2, CorrectCount = 4 },
                new Word { Id = 6, Text = "gentileza", Translation = "kindness", WrongCount = 2, CorrectCount = 1 },
                new Word { Id = 7, Text = "família", Translation = "family", WrongCount = 1, CorrectCount = 6 },
                new Word { Id = 8, Text = "amigo", Translation = "friend", WrongCount = 1, CorrectCount = 8 },
                new Word { Id = 9, Text = "casa", Translation = "house", WrongCount = 0, CorrectCount = 9 },
                new Word { Id = 10, Text = "escola", Translation = "school", WrongCount = 0, CorrectCount = 7 },
            };
        }
    }
}
