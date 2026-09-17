using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public class PracticeService : IPracticeService
    {
        // A Prática Focada sempre roda com 10 palavras (6 difíceis + 2 esquecidas + 2 dominadas).
        private const int FocusedPracticeLimit = 10;

        // O dicionário mostra a coleção completa do usuário, sem recorte.
        private static readonly int? DictionaryWordsLimit = null;

        private readonly IPracticeClient _practiceClient;

        public PracticeService(IPracticeClient practiceClient)
        {
            _practiceClient = practiceClient;
        }

        public async Task<List<PracticeWordModel>> GetFocusedPracticeWordsAsync()
        {
            // A API monta a sessão com o algoritmo 6-2-2 e já devolve a lista embaralhada.
            var words = await _practiceClient.GetFocusedWordsAsync(0, FocusedPracticeLimit);

            return words.ToList();
        }

        public async Task<List<PracticeWordModel>> GetDictionaryAsync()
        {
            var words = await _practiceClient.GetWordsAsync(null, DictionaryWordsLimit);

            return words.ToList();
        }

        public async Task RegisterResultAsync(int wordId, bool correct)
        {
            await _practiceClient.RegisterResultAsync(new PracticeResultModel
            {
                WordId = wordId,
                Correct = correct
            });
        }

        public async Task<PracticeWordModel> RegisterWrongAttemptAsync(int wordId)
        {
            return await _practiceClient.RegisterWrongAttemptAsync(new PracticeWrongAttemptModel
            {
                WordId = wordId
            });
        }
    }
}
