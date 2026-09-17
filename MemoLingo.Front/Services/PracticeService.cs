using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public class PracticeService : IPracticeService
    {
        // Quantidade de palavras trazidas da API para a lista de prática.
        private const int PracticeWordsLimit = 20;

        // O dicionário mostra a coleção completa do usuário, sem recorte.
        private static readonly int? DictionaryWordsLimit = null;

        private readonly IPracticeClient _practiceClient;

        public PracticeService(IPracticeClient practiceClient)
        {
            _practiceClient = practiceClient;
        }

        public async Task<List<PracticeWordModel>> GetWordsForPracticeAsync()
        {
            // A API já devolve as palavras priorizando aquelas em que o usuário mais erra.
            var words = await _practiceClient.GetWordsAsync(null, PracticeWordsLimit);

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
