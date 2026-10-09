using MemoLingo.Api.Client;
using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public class RelativePronounService : IRelativePronounService
    {
        private const int SessionSize = 12;
        private readonly IRelativePronounsClient _relativePronounsClient;

        public RelativePronounService(IRelativePronounsClient relativePronounsClient)
        {
            _relativePronounsClient = relativePronounsClient;
        }

        public async Task<List<RelativePronounExerciseModel>> GetSessionAsync(RelativePronounExerciseType? exerciseType)
        {
            // A API prioriza os exercícios errados por último e já devolve a sessão embaralhada.
            var exercises = await _relativePronounsClient.GetSessionAsync(0, SessionSize, exerciseType);

            return exercises.ToList();
        }

        public Task<RelativePronounAnswerResultModel> SubmitBlanksAsync(int exerciseId, IEnumerable<string> answers)
        {
            return SubmitAsync(new RelativePronounAnswerModel
            {
                ExerciseId = exerciseId,
                Answers = answers.ToList()
            });
        }

        public Task<RelativePronounAnswerResultModel> SubmitSentenceAsync(int exerciseId, string sentence)
        {
            return SubmitAsync(new RelativePronounAnswerModel
            {
                ExerciseId = exerciseId,
                SubmittedSentence = sentence
            });
        }

        public Task<RelativePronounAnswerResultModel> SubmitMistakeAsync(int exerciseId, int mistakeIndex, string correction)
        {
            return SubmitAsync(new RelativePronounAnswerModel
            {
                ExerciseId = exerciseId,
                MistakeIndex = mistakeIndex,
                Correction = correction
            });
        }

        // Converte os erros de validação da API em uma mensagem simples para a tela.
        private async Task<RelativePronounAnswerResultModel> SubmitAsync(RelativePronounAnswerModel model)
        {
            try
            {
                return await _relativePronounsClient.SubmitAnswerAsync(model);
            }
            catch (MemoLingoException<ErrorResponseModel> ex)
            {
                var message = ex.Result?.Errors;
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? "Não foi possível verificar sua resposta." : message, ex);
            }
        }
    }
}
