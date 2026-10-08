using MemoLingo.Api.Models;
using MemoLingo.Application.Models;
using MemoLingo.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemoLingo.Api.Controllers
{
    [ApiController]
    [Route("api/section-tests")]
    public class SectionTestsController : ControllerBase
    {
        private readonly ISectionTestService _sectionTestService;

        public SectionTestsController(ISectionTestService sectionTestService)
        {
            _sectionTestService = sectionTestService;
        }

        [HttpPost("sections/{sectionId:int}/start")]
        [ProducesResponseType(typeof(SectionTestSessionModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseModel), 404)]
        public async Task<IActionResult> Start([FromRoute] int sectionId, [FromQuery] int? userId)
        {
            try
            {
                var session = await _sectionTestService.StartAsync(userId, sectionId);
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
                var result = await _sectionTestService.AnswerAsync(model);
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
        [ProducesResponseType(typeof(SectionTestResultModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseModel), 404)]
        public async Task<IActionResult> Complete([FromRoute] int studySessionId, [FromQuery] int? userId)
        {
            try
            {
                var result = await _sectionTestService.CompleteAsync(userId, studySessionId);
                return Ok(result);
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
