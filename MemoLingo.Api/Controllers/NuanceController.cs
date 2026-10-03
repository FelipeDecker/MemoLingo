using MemoLingo.Api.Models;
using MemoLingo.Application.Models;
using MemoLingo.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemoLingo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NuanceController : ControllerBase
    {
        private readonly INuanceService _nuanceService;

        public NuanceController(INuanceService nuanceService)
        {
            _nuanceService = nuanceService;
        }

        [HttpGet("session")]
        [ProducesResponseType(typeof(IEnumerable<NuanceExerciseModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 500)]
        public async Task<IActionResult> GetSession([FromQuery] int userId, [FromQuery] int take = 10)
        {
            var exercises = await _nuanceService.GetSessionAsync(userId, take);
            return Ok(exercises);
        }

        [HttpPost("answers")]
        [ProducesResponseType(typeof(NuanceAnswerResultModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> SubmitAnswer([FromBody] NuanceAnswerModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var result = await _nuanceService.SubmitAnswerAsync(model);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }
    }
}
