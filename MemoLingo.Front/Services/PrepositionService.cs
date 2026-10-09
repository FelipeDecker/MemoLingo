using MemoLingo.Api.Client;
using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public class PrepositionService : IPrepositionService
    {
        private const int SessionSize = 12;
        private readonly IPrepositionsClient _prepositionsClient;

        public PrepositionService(IPrepositionsClient prepositionsClient)
        {
            _prepositionsClient = prepositionsClient;
        }

        public async Task<List<PrepositionExerciseModel>> GetSessionAsync(PrepositionExerciseType? exerciseType)
        {
            // A API prioriza os exercícios errados por último e já devolve a sessão embaralhada.
            var exercises = await _prepositionsClient.GetSessionAsync(0, SessionSize, exerciseType);

            return exercises.ToList();
        }

        public Task<PrepositionAnswerResultModel> SubmitBlanksAsync(int exerciseId, IEnumerable<string> answers)
        {
            return SubmitAsync(new PrepositionAnswerModel
            {
                ExerciseId = exerciseId,
                Answers = answers.ToList()
            });
        }

        public Task<PrepositionAnswerResultModel> SubmitSentenceAsync(int exerciseId, string sentence)
        {
            return SubmitAsync(new PrepositionAnswerModel
            {
                ExerciseId = exerciseId,
                SubmittedSentence = sentence
            });
        }

        public Task<PrepositionAnswerResultModel> SubmitMistakeAsync(int exerciseId, int mistakeIndex, string correction)
        {
            return SubmitAsync(new PrepositionAnswerModel
            {
                ExerciseId = exerciseId,
                MistakeIndex = mistakeIndex,
                Correction = correction
            });
        }

        // Converte os erros de validação da API em uma mensagem simples para a tela.
        private async Task<PrepositionAnswerResultModel> SubmitAsync(PrepositionAnswerModel model)
        {
            try
            {
                return await _prepositionsClient.SubmitAnswerAsync(model);
            }
            catch (MemoLingoException<ErrorResponseModel> ex)
            {
                var message = ex.Result?.Errors;
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? "Não foi possível verificar sua resposta." : message, ex);
            }
        }
    }
}
