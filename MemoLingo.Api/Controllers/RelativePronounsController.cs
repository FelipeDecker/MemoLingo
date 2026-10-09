using MemoLingo.Api.Models;
using MemoLingo.Application.Models;
using MemoLingo.Application.Services;
using MemoLingo.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MemoLingo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RelativePronounsController : ControllerBase
    {
        private readonly IRelativePronounService _relativePronounService;

        public RelativePronounsController(IRelativePronounService relativePronounService)
        {
            _relativePronounService = relativePronounService;
        }

        [HttpGet("session")]
        [ProducesResponseType(typeof(IEnumerable<RelativePronounExerciseModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 500)]
        public async Task<IActionResult> GetSession([FromQuery] int userId, [FromQuery] int take = 10, [FromQuery] RelativePronounExerciseType? exerciseType = null)
        {
            var exercises = await _relativePronounService.GetSessionAsync(userId, take, exerciseType);
            return Ok(exercises);
        }

        [HttpPost("answers")]
        [ProducesResponseType(typeof(RelativePronounAnswerResultModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> SubmitAnswer([FromBody] RelativePronounAnswerModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var result = await _relativePronounService.SubmitAnswerAsync(model);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }
    }
}
