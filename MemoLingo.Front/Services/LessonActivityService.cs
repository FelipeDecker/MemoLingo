using MemoLingo.Api.Client;
using MemoLingo.Api.Client.Contracts;

namespace MemoLingo.Front.Services
{
    public class LessonActivityService : ILessonActivityService
    {
        private readonly ILessonsClient _lessonsClient;

        public LessonActivityService(ILessonsClient lessonsClient)
        {
            _lessonsClient = lessonsClient;
        }

        public Task<LessonSessionModel> StartAsync(int pathNodeId)
        {
            return ExecuteAsync(() => _lessonsClient.StartAsync(pathNodeId, null));
        }

        public Task<LessonAnswerResultModel> SubmitAnswerAsync(int studySessionId, LessonExerciseModel exercise, string answer, int responseTimeMs)
        {
            return ExecuteAsync(() => _lessonsClient.AnswerAsync(new LessonAnswerModel
            {
                StudySessionId = studySessionId,
                SentenceId = exercise.SentenceId,
                ExerciseType = exercise.ExerciseType,
                BlankWordId = exercise.BlankWordId,
                Answer = answer,
                ResponseTimeMs = responseTimeMs
            }));
        }

        public Task<LessonCompletionModel> CompleteAsync(int studySessionId)
        {
            return ExecuteAsync(() => _lessonsClient.CompleteAsync(studySessionId, null));
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
