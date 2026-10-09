using MemoLingo.Api.Models;
using MemoLingo.Application.Models;
using MemoLingo.Application.Services;
using MemoLingo.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MemoLingo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrepositionsController : ControllerBase
    {
        private readonly IPrepositionService _prepositionService;

        public PrepositionsController(IPrepositionService prepositionService)
        {
            _prepositionService = prepositionService;
        }

        [HttpGet("session")]
        [ProducesResponseType(typeof(IEnumerable<PrepositionExerciseModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 500)]
        public async Task<IActionResult> GetSession([FromQuery] int userId, [FromQuery] int take = 10, [FromQuery] PrepositionExerciseType? exerciseType = null)
        {
            var exercises = await _prepositionService.GetSessionAsync(userId, take, exerciseType);
            return Ok(exercises);
        }

        [HttpPost("answers")]
        [ProducesResponseType(typeof(PrepositionAnswerResultModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> SubmitAnswer([FromBody] PrepositionAnswerModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var result = await _prepositionService.SubmitAnswerAsync(model);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }
    }
}
