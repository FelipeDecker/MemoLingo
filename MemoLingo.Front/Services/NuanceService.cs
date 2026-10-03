using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public class NuanceService : INuanceService
    {
        private const int SessionSize = 10;
        private readonly INuanceClient _nuanceClient;

        public NuanceService(INuanceClient nuanceClient)
        {
            _nuanceClient = nuanceClient;
        }

        public async Task<List<NuanceExerciseModel>> GetSessionAsync()
        {
            // A API prioriza os grupos com erros recentes e já devolve a sessão embaralhada.
            var exercises = await _nuanceClient.GetSessionAsync(0, SessionSize);

            return exercises.ToList();
        }

        public async Task<NuanceAnswerResultModel> SubmitAnswerAsync(int exerciseId, string answer, int? wordId)
        {
            return await _nuanceClient.SubmitAnswerAsync(new NuanceAnswerModel
            {
                ExerciseId = exerciseId,
                SubmittedAnswer = answer,
                SubmittedWordId = wordId
            });
        }
    }
}
