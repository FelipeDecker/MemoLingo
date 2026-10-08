using MemoLingo.Api.Models;
using MemoLingo.Application.Models;
using MemoLingo.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemoLingo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 500)]
        public async Task<IActionResult> Get()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return BadRequest(new ErrorResponseModel { Errors = "Usuário não encontrado" });
            return Ok(user);
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> GetCurrent([FromQuery] int? userId)
        {
            var user = await _userService.GetCurrentAsync(userId);
            if (user == null) return BadRequest(new ErrorResponseModel { Errors = "Usuário não encontrado" });
            return Ok(user);
        }

        [HttpPut("me/learning-stats-mode")]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> UpdateLearningStatsMode([FromBody] UpdateLearningStatsModeModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var user = await _userService.UpdateLearningStatsModeAsync(model);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }

        [HttpPut("me/accept-missing-apostrophes")]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> UpdateAcceptMissingApostrophes([FromBody] UpdateAcceptMissingApostrophesModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var user = await _userService.UpdateAcceptMissingApostrophesAsync(model);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> Create([FromBody] UserModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var created = await _userService.CreateAsync(model);
                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> Update(int id, [FromBody] UserModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var updated = await _userService.UpdateAsync(id, model);
                if (!updated) return BadRequest(new ErrorResponseModel { Errors = "Usuário não encontrado" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }

            return Ok(await _userService.GetByIdAsync(id));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> Delete(int id)
        {
            var removed = await _userService.RemoveAsync(id);
            if (!removed) return BadRequest(new ErrorResponseModel { Errors = "Usuário não encontrado" });
            return Ok();
        }
    }
}
