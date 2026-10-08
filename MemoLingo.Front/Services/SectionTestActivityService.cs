using MemoLingo.Api.Client;
using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public class SectionTestActivityService : ISectionTestActivityService
    {
        private readonly ISectionTestsClient _sectionTestsClient;

        public SectionTestActivityService(ISectionTestsClient sectionTestsClient)
        {
            _sectionTestsClient = sectionTestsClient;
        }

        public Task<SectionTestSessionModel> StartAsync(int sectionId)
        {
            return ExecuteAsync(() => _sectionTestsClient.StartAsync(sectionId, null));
        }

        public Task<LessonAnswerResultModel> SubmitAnswerAsync(int studySessionId, LessonExerciseModel exercise, string answer, int responseTimeMs)
        {
            return ExecuteAsync(() => _sectionTestsClient.AnswerAsync(new LessonAnswerModel
            {
                StudySessionId = studySessionId,
                SentenceId = exercise.SentenceId,
                ExerciseType = exercise.ExerciseType,
                BlankWordId = exercise.BlankWordId,
                Answer = answer,
                ResponseTimeMs = responseTimeMs
            }));
        }

        public Task<SectionTestResultModel> CompleteAsync(int studySessionId)
        {
            return ExecuteAsync(() => _sectionTestsClient.CompleteAsync(studySessionId, null));
        }

        // Converte os erros de validação da API em uma mensagem simples para a tela.
        private static async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (MemoLingoException<ErrorResponseModel> ex)
            {
                var message = ex.Result?.Errors;
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? "Não foi possível concluir a operação." : message, ex);
            }
        }
    }
}
