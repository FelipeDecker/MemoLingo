using MemoLingo.Api.Models;
using MemoLingo.Application.Models;
using MemoLingo.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemoLingo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonsController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpPost("nodes/{pathNodeId:int}/start")]
        [ProducesResponseType(typeof(LessonSessionModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseModel), 404)]
        public async Task<IActionResult> Start([FromRoute] int pathNodeId, [FromQuery] int? userId)
        {
            try
            {
                var session = await _lessonService.StartAsync(userId, pathNodeId);
                return Ok(session);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponseModel { Errors = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }

        [HttpPost("answers")]
        [ProducesResponseType(typeof(LessonAnswerResultModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseModel), 404)]
        public async Task<IActionResult> Answer([FromBody] LessonAnswerModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var result = await _lessonService.AnswerAsync(model);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponseModel { Errors = ex.Message });
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }

        [HttpPost("sessions/{studySessionId:int}/complete")]
        [ProducesResponseType(typeof(LessonCompletionModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseModel), 404)]
        public async Task<IActionResult> Complete([FromRoute] int studySessionId, [FromQuery] int? userId)
        {
            try
            {
                var completion = await _lessonService.CompleteAsync(userId, studySessionId);
                return Ok(completion);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponseModel { Errors = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }
    }
}
