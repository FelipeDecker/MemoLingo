using MemoLingo.Api.Models;
using MemoLingo.Application.Models;
using MemoLingo.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemoLingo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PracticeController : ControllerBase
    {
        private readonly IPracticeService _practiceService;

        public PracticeController(IPracticeService practiceService)
        {
            _practiceService = practiceService;
        }

        [HttpGet("words")]
        [ProducesResponseType(typeof(IEnumerable<PracticeWordModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 500)]
        public async Task<IActionResult> GetWords([FromQuery] int? userId, [FromQuery] int? take)
        {
            var words = await _practiceService.GetWordsAsync(userId, take);
            return Ok(words);
        }

        [HttpPost("results")]
        [ProducesResponseType(typeof(PracticeWordModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> RegisterResult([FromBody] PracticeResultModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var word = await _practiceService.RegisterResultAsync(model);
                return Ok(word);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }

        [HttpPost("attempts/wrong")]
        [ProducesResponseType(typeof(PracticeWordModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> RegisterWrongAttempt([FromBody] PracticeWrongAttemptModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var word = await _practiceService.RegisterWrongAttemptAsync(model);
                return Ok(word);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }
    }
}
